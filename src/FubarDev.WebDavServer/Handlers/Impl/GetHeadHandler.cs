// <copyright file="GetHeadHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Props;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Live;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    public class GetHeadHandler : IGetHandler, IHeadHandler
    {
        private readonly IWebDavContext _context;

        public GetHeadHandler(IFileSystem fileSystem, IWebDavContext context)
        {
            _context = context;
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

            public WebDavPartialDocumentResult([NotNull] IDocument document, bool returnFile, IReadOnlyCollection<NormalizedRangeItem> rangeItems)
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
                        }
                        catch
                        {
                            content.Dispose();
                            throw;
                        }
                    }
                    else
                    {
                        // Multipart content
                        var multipart = new MultipartContent("multipart/byteranges");
                        try
                        {
                            var index = 0;
                            foreach (var rangeItem in _rangeItems)
                            {
                                var streamView = views[index++];
                                var partContent = new StreamContent(streamView);
                                partContent.Headers.ContentRange = new ContentRangeHeaderValue(rangeItem.From, rangeItem.To, _document.Length);
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
                    propValue.IfSome(v => content.Headers.ContentLanguage.Add(v));
                }

                string contentType;
                var contentTypeProp = properties.OfType<GetContentTypeProperty>().FirstOrDefault();
                if (contentTypeProp != null)
                {
                    contentType = await contentTypeProp.GetValueAsync(ct).ConfigureAwait(false);
                }
                else
                {
                    contentType = Utils.MimeTypesMap.DefaultMimeType;
                }

                content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            }

            private class StreamView : Stream
            {
                private readonly Stream _baseStream;
                private long _position;

                private StreamView(Stream baseStream, long startPosition, long length)
                {
                    _baseStream = baseStream;
                    Offset = startPosition;
                    Length = length;
                }

                public override bool CanRead { get; } = true;

                public override bool CanSeek => _baseStream.CanSeek;

                public override bool CanWrite { get; } = false;

                public override long Length { get; }

                public override long Position
                {
                    get
                    {
                        return _position;
                    }

                    set
                    {
                        if (_position == value)
                            return;
                        _baseStream.Seek(value - _position, SeekOrigin.Current);
                        _position = value;
                    }
                }

                private long Offset { get; }

                public static async Task<StreamView> CreateAsync(
                    Stream baseStream,
                    long position,
                    long length,
                    CancellationToken ct)
                {
                    if (baseStream.CanSeek)
                    {
                        baseStream.Seek(position, SeekOrigin.Begin);
                    }
                    else
                    {
                        await SkipAsync(baseStream, position, ct).ConfigureAwait(false);
                    }

                    return new StreamView(baseStream, position, length);
                }

                public override void Flush()
                {
                    _baseStream.Flush();
                }

                public override int Read(byte[] buffer, int offset, int count)
                {
                    var remaining = Math.Min(Length - _position, count);
                    var readCount = _baseStream.Read(buffer, offset, (int)remaining);
                    _position += readCount;
                    return readCount;
                }

                public override long Seek(long offset, SeekOrigin origin)
                {
                    long result;
                    switch (origin)
                    {
                        case SeekOrigin.Begin:
                            result = _baseStream.Seek(Offset + offset, origin);
                            _position = offset;
                            break;
                        case SeekOrigin.Current:
                            var newPos = Offset + _position + offset;
                            if (newPos < Offset)
                                newPos = Offset;
                            if (newPos > Offset + Length)
                                newPos = Offset + Length;
                            var newOffset = newPos - (Offset + _position);
                            result = _baseStream.Seek(newOffset, SeekOrigin.Current);
                            _position = newPos - Offset;
                            break;
                        case SeekOrigin.End:
                            result = _baseStream.Seek(Offset + Length + offset, SeekOrigin.Begin);
                            _position = Length + offset;
                            break;
                        default:
                            throw new InvalidOperationException();
                    }

                    return result;
                }

                public override void SetLength(long value)
                {
                    throw new NotSupportedException();
                }

                public override void Write(byte[] buffer, int offset, int count)
                {
                    throw new NotSupportedException();
                }

                protected override void Dispose(bool disposing)
                {
                    base.Dispose(disposing);
                    if (disposing)
                        _baseStream.Dispose();
                }

                private static async Task SkipAsync(Stream baseStream, long count, CancellationToken ct)
                {
                    var buffer = new byte[65536];
                    while (count != 0)
                    {
                        var blockSize = Math.Min(65536, count);
                        await baseStream.ReadAsync(buffer, 0, (int)blockSize, ct).ConfigureAwait(false);
                        count -= blockSize;
                    }
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
                    await content.CopyToAsync(response.Body).ConfigureAwait(false);
                }
            }

            private async Task SetPropertiesToContentHeaderAsync(HttpContent content, IReadOnlyCollection<IUntypedReadableProperty> properties, CancellationToken ct)
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
                    propValue.IfSome(v => content.Headers.ContentLanguage.Add(v));
                }

                string contentType;
                var contentTypeProp = properties.OfType<GetContentTypeProperty>().FirstOrDefault();
                if (contentTypeProp != null)
                {
                    contentType = await contentTypeProp.GetValueAsync(ct).ConfigureAwait(false);
                }
                else
                {
                    contentType = Utils.MimeTypesMap.DefaultMimeType;
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
