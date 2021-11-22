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

namespace FubarDev.WebDavServer.Handlers.Impl
{
    /// <summary>
    /// The implementation of the <see cref="IGetHandler"/> and <see cref="IHeadHandler"/> interfaces.
    /// </summary>
    public class GetHeadHandler : IGetHandler, IHeadHandler
    {
        private readonly IWebDavContextAccessor _contextAccessor;
        private readonly IGetCollectionHandler? _getCollectionHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetHeadHandler"/> class.
        /// </summary>
        /// <param name="fileSystem">The root file system.</param>
        /// <param name="contextAccessor">The WebDAV context accessor.</param>
        /// <param name="getCollectionHandler">The handler to get the contents of collections.</param>
        public GetHeadHandler(
            IFileSystem fileSystem,
            IWebDavContextAccessor contextAccessor,
            IGetCollectionHandler? getCollectionHandler = null)
        {
            _contextAccessor = contextAccessor;
            _getCollectionHandler = getCollectionHandler;
            FileSystem = fileSystem;
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; } = new[] { "GET", "HEAD" };

        /// <summary>
        /// Gets the root file system.
        /// </summary>
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
            string path,
            bool returnFile,
            CancellationToken cancellationToken)
        {
            var selectionResult = await FileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);

            var context = _contextAccessor.WebDavContext;
            if (selectionResult.IsMissing)
            {
                if (context.RequestHeaders.IfNoneMatch != null)
                {
                    throw new WebDavException(WebDavStatusCode.PreconditionFailed);
                }

                throw new WebDavException(WebDavStatusCode.NotFound);
            }

            await context.RequestHeaders
                .ValidateAsync(selectionResult.TargetEntry, cancellationToken).ConfigureAwait(false);

            if (selectionResult.ResultType == SelectionResultType.FoundCollection)
            {
                Debug.Assert(selectionResult.Collection != null, "selectionResult.Collection != null");

                if (returnFile)
                {
                    if (_getCollectionHandler == null)
                    {
                        throw new NotSupportedException();
                    }

                    // Gets the contents of the collection
                    return new WebDavCollectionResult(selectionResult.Collection)
                    {
                        ResponseStream = await _getCollectionHandler.GetCollectionAsync(
                            selectionResult.Collection,
                            cancellationToken),
                    };
                }

                return new WebDavCollectionResult(selectionResult.Collection);
            }

            Debug.Assert(selectionResult.Document != null, "selectionResult.Document != null");

            var doc = selectionResult.Document;
            var rangeHeader = context.RequestHeaders.Range;
            if (rangeHeader != null)
            {
                if (rangeHeader.Unit != "bytes")
                {
                    throw new NotSupportedException();
                }

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
