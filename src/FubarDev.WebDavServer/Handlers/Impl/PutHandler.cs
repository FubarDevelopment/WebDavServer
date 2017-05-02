// <copyright file="PutHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.Utils;

using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    /// <summary>
    /// Implementation of the <see cref="IPutHandler"/> interface
    /// </summary>
    public class PutHandler : IPutHandler
    {
        private readonly IFileSystem _fileSystem;
        private readonly IWebDavContext _context;
        private readonly ILogger<PutHandler> _logger;
        private readonly ArrayPool<byte> _buffers = ArrayPool<byte>.Shared;

        /// <summary>
        /// Initializes a new instance of the <see cref="PutHandler"/> class.
        /// </summary>
        /// <param name="fileSystem">The root file system</param>
        /// <param name="context">The WebDAV request context</param>
        /// <param name="logger">The logger</param>
        public PutHandler(IFileSystem fileSystem, IWebDavContext context, ILogger<PutHandler> logger)
        {
            _fileSystem = fileSystem;
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; } = new[] { "PUT" };

        /// <inheritdoc />
        public async Task<IWebDavResult> PutAsync(string path, Stream data, CancellationToken cancellationToken)
        {
            var selectionResult = await _fileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);
            if (selectionResult.ResultType == SelectionResultType.MissingCollection)
                throw new WebDavException(WebDavStatusCode.NotFound);
            if (selectionResult.ResultType == SelectionResultType.FoundCollection)
                throw new WebDavException(WebDavStatusCode.MethodNotAllowed);

            if (selectionResult.IsMissing)
            {
                if (_context.RequestHeaders.IfNoneMatch != null)
                    throw new WebDavException(WebDavStatusCode.PreconditionFailed);
            }
            else
            {
                await _context.RequestHeaders
                    .ValidateAsync(selectionResult.TargetEntry, cancellationToken).ConfigureAwait(false);
            }

            var lockRequirements = new Lock(
                new Uri(path, UriKind.Relative),
                _context.RelativeRequestUrl,
                false,
                new XElement(WebDavXml.Dav + "owner", _context.User.Identity.Name),
                LockAccessType.Write,
                LockShareMode.Shared,
                TimeoutHeader.Infinite);
            var lockManager = _fileSystem.LockManager;
            var tempLock = lockManager == null
                ? new ImplicitLock(true)
                : await lockManager.LockImplicitAsync(
                        _fileSystem,
                        _context.RequestHeaders.If?.Lists,
                        lockRequirements,
                        cancellationToken)
                    .ConfigureAwait(false);
            if (!tempLock.IsSuccessful)
                return tempLock.CreateErrorResponse();

            try
            {
                IDocument document;
                if (selectionResult.ResultType == SelectionResultType.FoundDocument)
                {
                    Debug.Assert(selectionResult.Document != null, "selectionResult.Document != null");
                    document = selectionResult.Document;
                }
                else
                {
                    Debug.Assert(
                        selectionResult.ResultType == SelectionResultType.MissingDocumentOrCollection,
                        "selectionResult.ResultType == SelectionResultType.MissingDocumentOrCollection");
                    Debug.Assert(selectionResult.MissingNames != null, "selectionResult.PathEntries != null");
                    Debug.Assert(selectionResult.MissingNames.Count == 1, "selectionResult.MissingNames.Count == 1");
                    Debug.Assert(selectionResult.Collection != null, "selectionResult.Collection != null");
                    var newName = selectionResult.MissingNames.Single();
                    document =
                        await selectionResult.Collection.CreateDocumentAsync(newName, cancellationToken)
                            .ConfigureAwait(false);
                }

                using (var fileStream = await document.CreateAsync(cancellationToken).ConfigureAwait(false))
                {
                    var contentLength = _context.RequestHeaders.ContentLength;
                    if (contentLength == null)
                    {
                        _logger.LogInformation("Writing data without content length");
                        await data.CopyToAsync(fileStream).ConfigureAwait(false);
                    }
                    else
                    {
                        _logger.LogInformation("Writing data with content length {0}", contentLength.Value);
                        await Copy(data, fileStream, contentLength.Value, cancellationToken).ConfigureAwait(false);
                    }
                }

                var docPropertyStore = document.FileSystem.PropertyStore;
                if (docPropertyStore != null)
                {
                    await docPropertyStore.UpdateETagAsync(document, cancellationToken).ConfigureAwait(false);

                    if (selectionResult.ResultType == SelectionResultType.FoundDocument)
                    {
                        Debug.Assert(selectionResult.Document != null, "selectionResult.Document != null");
                        await docPropertyStore.RemoveAsync(selectionResult.Document, cancellationToken)
                            .ConfigureAwait(false);
                    }
                }

                var parent = document.Parent;
                Debug.Assert(parent != null, "parent != null");
                var parentPropStore = parent.FileSystem.PropertyStore;
                if (parentPropStore != null)
                {
                    await parentPropStore.UpdateETagAsync(parent, cancellationToken).ConfigureAwait(false);
                }

                return
                    new WebDavResult(selectionResult.ResultType != SelectionResultType.FoundDocument
                        ? WebDavStatusCode.Created
                        : WebDavStatusCode.OK);
            }
            finally
            {
                await tempLock.DisposeAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task Copy(Stream source, Stream destination, long contentLength, CancellationToken cancellationToken)
        {
#if DEBUG
            var bufferSize = 4096;
#else
            var bufferSize = 65536;
#endif
            var buffer = _buffers.Rent(bufferSize);
            try
            {
                var maxDelay = TimeSpan.FromMilliseconds(200);
                var sw = new Stopwatch();
                var totalReadCount = 0L;
                var remaining = contentLength;
                while (remaining != 0)
                {
                    var copySize = (int)Math.Min(remaining, bufferSize);
                    sw.Restart();
                    var readCount = await source.ReadAsync(buffer, 0, copySize, cancellationToken).ConfigureAwait(false);
                    await destination.WriteAsync(buffer, 0, readCount, cancellationToken).ConfigureAwait(false);
                    var elapsed = sw.Elapsed;
                    if (readCount == bufferSize && elapsed < maxDelay && bufferSize < 0x4000000)
                    {
                        _buffers.Return(buffer);
                        bufferSize *= 2;
                        _logger.LogTrace("Increased buffer size to {0}", bufferSize);
                        buffer = _buffers.Rent(bufferSize);
                    }

                    remaining -= readCount;
                    totalReadCount += readCount;
                    _logger.LogDebug("Wrote {0} bytes", totalReadCount);
                }
            }
            finally
            {
                _buffers.Return(buffer);
            }
        }
    }
}
