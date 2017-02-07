// <copyright file="GetHeadHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Props.Store;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    public class GetHeadHandler : IGetHandler, IHeadHandler
    {
        public GetHeadHandler(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; } = new[] { "GET", "HEAD" };

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

        private async Task<IWebDavResult> HandleAsync(string path, bool returnFile, CancellationToken cancellationToken)
        {
            var searchResult = await FileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);
            if (searchResult.IsMissing)
                throw new WebDavException(WebDavStatusCode.NotFound);
            if (searchResult.ResultType == SelectionResultType.FoundCollection)
            {
                if (returnFile)
                    throw new NotSupportedException();
                Debug.Assert(searchResult.Collection != null, "searchResult.Collection != null");
                return new WebDavCollectionResult(searchResult.Collection);
            }

            Debug.Assert(searchResult.Document != null, "searchResult.Document != null");
            return new WebDavDocumentResult(FileSystem.PropertyStore, searchResult.Document, returnFile);
        }

        private class WebDavDocumentResult : WebDavResult
        {
            [CanBeNull]
            private readonly IPropertyStore _propertyStore;

            [NotNull]
            private readonly IDocument _document;

            private readonly bool _returnFile;

            public WebDavDocumentResult([CanBeNull] IPropertyStore propertyStore, [NotNull] IDocument document, bool returnFile)
                : base(WebDavStatusCode.OK)
            {
                _propertyStore = propertyStore;
                _document = document;
                _returnFile = returnFile;
            }

            public override async Task ExecuteResultAsync(IWebDavResponse response, CancellationToken ct)
            {
                await base.ExecuteResultAsync(response, ct).ConfigureAwait(false);
                if (_propertyStore != null)
                {
                    var etag = await _propertyStore.GetETagAsync(_document, ct).ConfigureAwait(false);
                    response.Headers["ETag"] = new[] { etag.ToString() };
                }

                response.Headers["Last-Modified"] = new[] { _document.LastWriteTimeUtc.ToString("R") };

                if (!_returnFile)
                    return;

                response.ContentType = "application/octet-stream";

                using (var stream = await _document.OpenReadAsync(ct).ConfigureAwait(false))
                {
                    await stream.CopyToAsync(response.Body, 65536, ct).ConfigureAwait(false);
                }
            }
        }

        private class WebDavCollectionResult : WebDavResult
        {
            [NotNull]
            private readonly ICollection _collection;

            public WebDavCollectionResult([NotNull] ICollection collection)
                : base(WebDavStatusCode.OK)
            {
                _collection = collection;
            }

            public override async Task ExecuteResultAsync(IWebDavResponse response, CancellationToken ct)
            {
                await base.ExecuteResultAsync(response, ct).ConfigureAwait(false);
                response.Headers["Last-Modified"] = new[] { _collection.LastWriteTimeUtc.ToString("R") };
            }
        }
    }
}
