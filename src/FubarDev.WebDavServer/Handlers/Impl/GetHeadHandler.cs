// <copyright file="GetHeadHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.Props;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Live;
using FubarDev.WebDavServer.Utils;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    public class GetHeadHandler : IGetHandler, IHeadHandler
    {
        [NotNull]
        private readonly IWebDavContext _context;

        public GetHeadHandler([NotNull] IFileSystem fileSystem, [NotNull] IWebDavContext context)
        {
            _context = context;
            FileSystem = fileSystem;
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; } = new[] { "GET", "HEAD" };

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

        private async Task<IWebDavResult> HandleAsync([NotNull] string path, bool returnFile, CancellationToken cancellationToken)
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

            var doc = searchResult.Document;
            var rangeHeader = _context.RequestHeaders.Range;
            if (rangeHeader != null)
            {
                if (rangeHeader.Unit != "bytes")
                    throw new NotSupportedException();

                var rangeItems = rangeHeader.Normalize(doc.Length);
                if (rangeItems.Any(x => x.Length < 0 || x.To >= doc.Length))
                    return new WebDavResult(WebDavStatusCode.RequestedRangeNotSatisfiable);

                return new WebDavPartialDocumentResult(doc, returnFile, rangeItems);
            }

            return new WebDavFullDocumentResult(doc, returnFile);
        }

        private class WebDavPartialDocumentResult : WebDavResult
        {
            [NotNull]
            private readonly IDocument _document;

            private readonly bool _returnFile;

            [NotNull]
            private readonly IReadOnlyCollection<NormalizedRangeItem> _rangeItems;

            public WebDavPartialDocumentResult([NotNull] IDocument document, bool returnFile, [NotNull] IReadOnlyCollection<NormalizedRangeItem> rangeItems)
                : base(WebDavStatusCode.PartialContent)
            {
                _document = document;
                _returnFile = returnFile;
                _rangeItems = rangeItems;
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

                if (!_returnFile)
                {
                    var lastModifiedProp = properties.OfType<LastModifiedProperty>().FirstOrDefault();
                    if (lastModifiedProp != null)
                    {
                        var propValue = await lastModifiedProp.GetValueAsync(ct).ConfigureAwait(false);
                        response.Headers["Last-Modified"] = new[] { propValue.ToString("R") };
                    }

                    return;
                }

                var views = new List<StreamView>();
                try
                {
                    foreach (var rangeItem in _rangeItems)
                    {
                        var baseStream = await _document.OpenReadAsync(ct).ConfigureAwait(false);
                        var streamView = await StreamView
                            .CreateAsync(baseStream, rangeItem.From, rangeItem.Length, ct)
                            .ConfigureAwait(false);
                        views.Add(streamView);
                    }

                    string contentType;
                    var contentTypeProp = properties.OfType<GetContentTypeProperty>().FirstOrDefault();
                    if (contentTypeProp != null)
                    {
                        contentType = await contentTypeProp.GetValueAsync(ct).ConfigureAwait(false);
                    }
                    else
                    {
                        contentType = MimeTypesMap.DefaultMimeType;
                    }

                    HttpContent content;
                    if (_rangeItems.Count == 1)
                    {
                        // No multipart content
                        var rangeItem = _rangeItems.Single();
                        var streamView = views.Single();
                        content = new StreamContent(streamView);
                        try
                        {
                            content.Headers.ContentRange = new ContentRangeHeaderValue(rangeItem.From, rangeItem.To, _document.Length);
                            content.Headers.ContentLength = rangeItem.Length;
                        }
                        catch
                        {
                            content.Dispose();
                            throw;
                        }

                        content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                    }
                    else
                    {
                        // Multipart content
                        var multipart = new MultipartContent("byteranges");
                        try
                        {
                            var index = 0;
                            foreach (var rangeItem in _rangeItems)
                            {
                                var streamView = views[index++];
                                var partContent = new StreamContent(streamView);
                                partContent.Headers.ContentRange = new ContentRangeHeaderValue(rangeItem.From, rangeItem.To, _document.Length);
                                partContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                                partContent.Headers.ContentLength = rangeItem.Length;
                                multipart.Add(partContent);
                            }
                        }
                        catch
                        {
                            multipart.Dispose();
                            throw;
                        }

                        content = multipart;
                    }

                    using (content)
                    {
                        await SetPropertiesToContentHeaderAsync(content, properties, ct)
                            .ConfigureAwait(false);

                        foreach (var header in content.Headers)
                            response.Headers.Add(header.Key, header.Value.ToArray());

                        await content.CopyToAsync(response.Body).ConfigureAwait(false);
                    }
                }
                finally
                {
                    foreach (var streamView in views)
                    {
                        streamView.Dispose();
                    }
                }
            }

            private async Task SetPropertiesToContentHeaderAsync(
                HttpContent content,
                IReadOnlyCollection<IUntypedReadableProperty> properties,
                CancellationToken ct)
            {
                var lastModifiedProp = properties.OfType<LastModifiedProperty>().FirstOrDefault();
                if (lastModifiedProp != null)
                {
                    var propValue = await lastModifiedProp.GetValueAsync(ct).ConfigureAwait(false);
                    content.Headers.LastModified = new DateTimeOffset(propValue);
                }

                var contentLanguageProp = properties.OfType<GetContentLanguageProperty>().FirstOrDefault();
                if (contentLanguageProp != null)
                {
                    var propValue = await contentLanguageProp.TryGetValueAsync(ct).ConfigureAwait(false);
                    if (propValue.Item1)
                        content.Headers.ContentLanguage.Add(propValue.Item2);
                }
            }
        }

        private class WebDavFullDocumentResult : WebDavResult
        {
            [NotNull]
            private readonly IDocument _document;

            private readonly bool _returnFile;

            public WebDavFullDocumentResult([NotNull] IDocument document, bool returnFile)
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

                if (!_returnFile)
                {
                    var lastModifiedProp = properties.OfType<LastModifiedProperty>().FirstOrDefault();
                    if (lastModifiedProp != null)
                    {
                        var propValue = await lastModifiedProp.GetValueAsync(ct).ConfigureAwait(false);
                        response.Headers["Last-Modified"] = new[] { propValue.ToString("R") };
                    }

                    return;
                }

                using (var stream = await _document.OpenReadAsync(ct).ConfigureAwait(false))
                {
                    var content = new StreamContent(stream);
                    await SetPropertiesToContentHeaderAsync(content, properties, ct).ConfigureAwait(false);

                    foreach (var header in content.Headers)
                        response.Headers.Add(header.Key, header.Value.ToArray());

                    await content.CopyToAsync(response.Body).ConfigureAwait(false);
                }
            }

            private async Task SetPropertiesToContentHeaderAsync([NotNull] HttpContent content, [NotNull][ItemNotNull] IReadOnlyCollection<IUntypedReadableProperty> properties, CancellationToken ct)
            {
                var lastModifiedProp = properties.OfType<LastModifiedProperty>().FirstOrDefault();
                if (lastModifiedProp != null)
                {
                    var propValue = await lastModifiedProp.GetValueAsync(ct).ConfigureAwait(false);
                    content.Headers.LastModified = new DateTimeOffset(propValue);
                }

                var contentLanguageProp = properties.OfType<GetContentLanguageProperty>().FirstOrDefault();
                if (contentLanguageProp != null)
                {
                    var propValue = await contentLanguageProp.TryGetValueAsync(ct).ConfigureAwait(false);
                    if (propValue.Item1)
                        content.Headers.ContentLanguage.Add(propValue.Item2);
                }

                string contentType;
                var contentTypeProp = properties.OfType<GetContentTypeProperty>().FirstOrDefault();
                if (contentTypeProp != null)
                {
                    contentType = await contentTypeProp.GetValueAsync(ct).ConfigureAwait(false);
                }
                else
                {
                    contentType = MimeTypesMap.DefaultMimeType;
                }

                content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

                var contentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = _document.Name,
                    FileNameStar = _document.Name,
                };

                content.Headers.ContentDisposition = contentDisposition;
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
