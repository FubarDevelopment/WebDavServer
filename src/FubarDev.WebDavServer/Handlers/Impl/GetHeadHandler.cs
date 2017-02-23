// <copyright file="GetHeadHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Handlers.Impl.GetResults;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Utils;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    /// <summary>
    /// The implementation of the <see cref="IGetHandler"/> and <see cref="IHeadHandler"/> interfaces
    /// </summary>
    public class GetHeadHandler : IGetHandler, IHeadHandler
    {
        [NotNull]
        private readonly IWebDavContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetHeadHandler"/> class.
        /// </summary>
        /// <param name="fileSystem">The root file system</param>
        /// <param name="context">The WebDAV context</param>
        public GetHeadHandler([NotNull] IFileSystem fileSystem, [NotNull] IWebDavContext context)
        {
            _context = context;
            FileSystem = fileSystem;
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; } = new[] { "GET", "HEAD" };

        /// <summary>
        /// Gets the root file system
        /// </summary>
        [NotNull]
        public IFileSystem FileSystem { get; }

        /// <inheritdoc />
        public Task<IWebDavResult> GetAsync(string path, CancellationToken cancellationToken)
        {
            return HandleAsync(path, true, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> HeadAsync(string path, CancellationToken cancellationToken)
        {
            return HandleAsync(path, false, cancellationToken);
        }

        private async Task<IWebDavResult> HandleAsync(
            [NotNull] string path,
            bool returnFile,
            CancellationToken cancellationToken)
        {
            var selectionResult = await FileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);

            if (selectionResult.IsMissing)
            {
                if (_context.RequestHeaders.IfNoneMatch != null)
                    throw new WebDavException(WebDavStatusCode.PreconditionFailed);

                throw new WebDavException(WebDavStatusCode.NotFound);
            }

            await _context.RequestHeaders
                .ValidateAsync(selectionResult.TargetEntry, cancellationToken).ConfigureAwait(false);

            if (selectionResult.ResultType == SelectionResultType.FoundCollection)
            {
                if (returnFile)
                    throw new NotSupportedException();
                Debug.Assert(selectionResult.Collection != null, "selectionResult.Collection != null");
                return new WebDavCollectionResult(selectionResult.Collection);
            }

            Debug.Assert(selectionResult.Document != null, "selectionResult.Document != null");

            var doc = selectionResult.Document;
            var rangeHeader = _context.RequestHeaders.Range;
            if (rangeHeader != null)
            {
                if (rangeHeader.Unit != "bytes")
                    throw new NotSupportedException();

                var rangeItems = rangeHeader.Normalize(doc.Length);
                if (rangeItems.Any(x => x.Length < 0 || x.To >= doc.Length))
                {
                    return new WebDavResult(WebDavStatusCode.RequestedRangeNotSatisfiable)
                    {
                        Headers =
                        {
                            ["Content-Range"] = new[] { $"bytes */{doc.Length}" },
                        },
                    };
                }

                return new WebDavPartialDocumentResult(doc, returnFile, rangeItems);
            }

            return new WebDavFullDocumentResult(doc, returnFile);
        }
    }
}
