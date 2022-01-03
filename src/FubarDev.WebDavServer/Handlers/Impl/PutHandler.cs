// <copyright file="PutHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.Props;
using FubarDev.WebDavServer.Utils;

using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    /// <summary>
    /// Implementation of the <see cref="IPutHandler"/> interface.
    /// </summary>
    public class PutHandler : IPutHandler
    {
        private readonly IFileSystem _fileSystem;

        private readonly IWebDavContextAccessor _contextAccessor;

        private readonly IImplicitLockFactory _implicitLockFactory;

        private readonly IEntryPropertyInitializer _entryPropertyInitializer;

        private readonly IBufferPoolFactory _bufferPoolFactory;

        private readonly ILogger<PutHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PutHandler"/> class.
        /// </summary>
        /// <param name="fileSystem">The root file system.</param>
        /// <param name="contextAccessor">The WebDAV request context accessor.</param>
        /// <param name="implicitLockFactory">A factory to create implicit locks.</param>
        /// <param name="entryPropertyInitializer">The property initializer.</param>
        /// <param name="bufferPoolFactory">A buffer pool factory.</param>
        /// <param name="logger">The logger.</param>
        public PutHandler(
            IFileSystem fileSystem,
            IWebDavContextAccessor contextAccessor,
            IImplicitLockFactory implicitLockFactory,
            IEntryPropertyInitializer entryPropertyInitializer,
            IBufferPoolFactory bufferPoolFactory,
            ILogger<PutHandler> logger)
        {
            _fileSystem = fileSystem;
            _contextAccessor = contextAccessor;
            _implicitLockFactory = implicitLockFactory;
            _entryPropertyInitializer = entryPropertyInitializer;
            _bufferPoolFactory = bufferPoolFactory;
            _logger = logger;
        }

        /// <summary>
        /// The kind of PUT operation.
        /// </summary>
        private enum PutOperation
        {
            /// <summary>
            /// A new document will be created.
            /// </summary>
            Create,

            /// <summary>
            /// The current document will be replaced.
            /// </summary>
            Overwrite,

            /// <summary>
            /// The existing document will be modified.
            /// </summary>
            Modify,
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; } = new[] { "PUT" };

        /// <inheritdoc />
        public async Task<IWebDavResult> PutAsync(
            string path,
            Stream data,
            CancellationToken cancellationToken)
        {
            var selectionResult = await _fileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);
            if (selectionResult.ResultType == SelectionResultType.MissingCollection)
            {
                throw new WebDavException(WebDavStatusCode.NotFound);
            }

            if (selectionResult.ResultType == SelectionResultType.FoundCollection)
            {
                throw new WebDavException(WebDavStatusCode.MethodNotAllowed);
            }

            if (selectionResult.ResultType == SelectionResultType.MissingDocumentOrCollection &&
                selectionResult.MissingNames.Count > 1)
            {
                throw new WebDavException(WebDavStatusCode.NotFound);
            }

            var context = _contextAccessor.WebDavContext;
            if (selectionResult.IsMissing)
            {
                if (context.RequestHeaders.IfNoneMatch != null)
                {
                    throw new WebDavException(WebDavStatusCode.PreconditionFailed);
                }
            }
            else
            {
                await context.RequestHeaders
                    .ValidateAsync(selectionResult.TargetEntry, cancellationToken).ConfigureAwait(false);
            }

            var lockRequirements = new Lock(
                new Uri(path, UriKind.Relative),
                context.HrefUrl,
                false,
                context.User.Identity.GetOwner(),
                context.User.Identity.GetOwnerHref(),
                LockAccessType.Write,
                LockShareMode.Exclusive,
                TimeoutHeader.Infinite);
            var tempLock = await _implicitLockFactory.CreateAsync(lockRequirements, cancellationToken)
                .ConfigureAwait(false);
            if (!tempLock.IsSuccessful)
            {
                return tempLock.CreateErrorResponse();
            }

            PutOperation operation;
            long? startPosition = null;
            if (selectionResult.ResultType == SelectionResultType.FoundDocument)
            {
                if (context.RequestHeaders.Headers.TryGetValue(HeaderNames.ContentRange, out var range))
                {
                    if (range.Count > 1)
                    {
                        // We only allow 1 byte range (max)
                        return new WebDavResult(WebDavStatusCode.RequestedRangeNotSatisfiable);
                    }

                    if (range.Count != 0)
                    {
                        var contentRange = ContentRangeHeaderValue.Parse(range.Single());
                        startPosition = contentRange.HasRange
                            ? contentRange.From
                            : 0;
                        operation = PutOperation.Modify;
                    }
                    else
                    {
                        operation = PutOperation.Overwrite;
                    }
                }
                else
                {
                    operation = PutOperation.Overwrite;
                }
            }
            else
            {
                operation = PutOperation.Create;
            }

            try
            {
                IDocument document;
                if (operation != PutOperation.Create)
                {
                    // Use the existing document
                    Debug.Assert(selectionResult.Document != null, "selectionResult.Document != null");
                    document = selectionResult.Document;
                }
                else
                {
                    // Create a new document
                    Debug.Assert(
                        selectionResult.ResultType == SelectionResultType.MissingDocumentOrCollection,
                        "selectionResult.ResultType == SelectionResultType.MissingDocumentOrCollection");
                    Debug.Assert(selectionResult.MissingNames != null, "selectionResult.PathEntries != null");
                    Debug.Assert(selectionResult.MissingNames.Count == 1, "selectionResult.MissingNames.Count == 1");
                    Debug.Assert(selectionResult.Collection != null, "selectionResult.Collection != null");
                    var newName = selectionResult.MissingNames.Single();
                    document = await selectionResult.Collection.CreateDocumentAsync(newName, cancellationToken)
                        .ConfigureAwait(false);
                }

                Debug.Assert(document != null, nameof(document) + " != null");
                var fileStream = operation == PutOperation.Modify
                    ? await document.OpenWriteAsync(startPosition ?? 0, cancellationToken)
                        .ConfigureAwait(false)
                    : await document.CreateAsync(cancellationToken)
                        .ConfigureAwait(false);
                using (fileStream)
                {
                    var contentLength = context.RequestHeaders.ContentLength;
                    if (contentLength == null)
                    {
                        _logger.LogInformation("Writing data without content length");
                        await data.CopyToAsync(fileStream, SystemInfo.CopyBufferSize, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Writing data with content length {ContentLength}",
                            contentLength.Value);
                        await Copy(data, fileStream, contentLength.Value, cancellationToken).ConfigureAwait(false);
                    }
                }

                var docPropertyStore = document.FileSystem.PropertyStore;
                if (docPropertyStore != null)
                {
                    // Remove the old dead properties first
                    if (operation == PutOperation.Overwrite)
                    {
                        Debug.Assert(selectionResult.Document != null, "selectionResult.Document != null");
                        await docPropertyStore.RemoveAsync(selectionResult.Document, cancellationToken)
                            .ConfigureAwait(false);
                    }

                    await docPropertyStore.UpdateETagAsync(document, cancellationToken).ConfigureAwait(false);
                    await _entryPropertyInitializer.CreatePropertiesAsync(
                            document,
                            docPropertyStore,
                            context,
                            cancellationToken)
                        .ConfigureAwait(false);
                }

                // Update the ETag of the collection if the file was created
                if (operation == PutOperation.Create)
                {
                    var parent = document.Parent;
                    Debug.Assert(parent != null, "parent != null");
                    var parentPropStore = parent.FileSystem.PropertyStore;
                    if (parentPropStore != null)
                    {
                        await parentPropStore.UpdateETagAsync(parent, cancellationToken).ConfigureAwait(false);
                    }

                    return new WebDavResult(WebDavStatusCode.Created);
                }

                return new WebDavResult(WebDavStatusCode.NoContent);
            }
            finally
            {
                await tempLock.DisposeAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task Copy(Stream source, Stream destination, long contentLength, CancellationToken cancellationToken)
        {
            using var pool = _bufferPoolFactory.CreatePool();
            var readCount = 0;
            var totalReadCount = 0L;
            var remaining = contentLength;
            while (remaining != 0)
            {
                var buffer = pool.GetBuffer(readCount);
                var copySize = (int)Math.Min(remaining, buffer.Length);
                readCount = await source.ReadAsync(buffer, 0, copySize, cancellationToken).ConfigureAwait(false);
                await destination.WriteAsync(buffer, 0, readCount, cancellationToken).ConfigureAwait(false);

                remaining -= readCount;
                totalReadCount += readCount;
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("Wrote {Count} bytes", totalReadCount);
                }
            }
        }
    }
}
