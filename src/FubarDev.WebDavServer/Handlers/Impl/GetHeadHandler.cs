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
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Live;

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
            return new WebDavDocumentResult(searchResult.Document, returnFile);
        }

        private class WebDavDocumentResult : WebDavResult
        {
            [NotNull]
            private readonly IDocument _document;

            private readonly bool _returnFile;

            public WebDavDocumentResult([NotNull] IDocument document, bool returnFile)
                : base(WebDavStatusCode.OK)
            {
                _document = document;
                _returnFile = returnFile;
            }

            public override async Task ExecuteResultAsync(IWebDavResponse response, CancellationToken ct)
            {
                await base.ExecuteResultAsync(response, ct).ConfigureAwait(false);

                var properties = await _document.GetProperties(int.MaxValue).ToList(ct).ConfigureAwait(false);
                var etagProperty = properties.OfType<GetETagProperty>().FirstOrDefault();
                if (etagProperty != null)
                {
                    var propValue = await etagProperty.GetValueAsync(ct).ConfigureAwait(false);
                    response.Headers["ETag"] = new[] { propValue.ToString() };
                }

                var lastModifiedProp = properties.OfType<LastModifiedProperty>().FirstOrDefault();
                if (lastModifiedProp != null)
                {
                    var propValue = await lastModifiedProp.GetValueAsync(ct).ConfigureAwait(false);
                    response.Headers["Last-Modified"] = new[] { propValue.ToString("R") };
                }

                if (!_returnFile)
                    return;

                var contentLanguage = properties.OfType<GetContentLanguageProperty>().FirstOrDefault();
                if (contentLanguage != null)
                {
                    var propValue = await contentLanguage.TryGetValueAsync(ct).ConfigureAwait(false);
                    propValue.IfSome(v => response.Headers["Content-Language"] = new[] { v });
                }

                var contentType = properties.OfType<GetContentTypeProperty>().FirstOrDefault();
                if (contentType != null)
                {
                    var propValue = await contentType.GetValueAsync(ct).ConfigureAwait(false);
                    response.ContentType = propValue;
                }
                else
                {
                    response.ContentType = Utils.MimeTypesMap.DefaultMimeType;
                }

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
