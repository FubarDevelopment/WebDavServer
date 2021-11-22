// <copyright file="WebDavPartialDocumentResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
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

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.WebDavServer.Handlers.Impl.GetResults
{
    internal class WebDavPartialDocumentResult : WebDavResult
    {
        private readonly IDocument _document;

        private readonly bool _returnFile;

        private readonly IReadOnlyCollection<NormalizedRangeItem> _rangeItems;

        public WebDavPartialDocumentResult(IDocument document, bool returnFile, IReadOnlyCollection<NormalizedRangeItem> rangeItems)
            : base(WebDavStatusCode.PartialContent)
        {
            _document = document;
            _returnFile = returnFile;
            _rangeItems = rangeItems;
        }

        public override async Task ExecuteResultAsync(IWebDavResponse response, CancellationToken ct)
        {
            await base.ExecuteResultAsync(response, ct).ConfigureAwait(false);

            response.Headers["Accept-Ranges"] = new[] { "bytes" };

            var deadPropertyFactory = response.Context.RequestServices.GetRequiredService<IDeadPropertyFactory>();
            var properties = await _document.GetProperties(deadPropertyFactory).ToListAsync(ct).ConfigureAwait(false);
            var etagProperty = properties
                .OfType<ITypedReadableProperty<EntityTag>>()
                .FirstOrDefault(x => x.Name == GetETagProperty.PropertyName);
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

                if (_rangeItems.Count == 1)
                {
                    // No multipart content
                    var rangeItem = _rangeItems.Single();
                    var streamView = views.Single();
                    using (var streamContent = new StreamContent(streamView))
                    {
                        streamContent.Headers.ContentRange = new ContentRangeHeaderValue(
                            rangeItem.From,
                            rangeItem.To,
                            _document.Length);
                        streamContent.Headers.ContentLength = rangeItem.Length;

                        streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

                        await SetPropertiesToContentHeaderAsync(streamContent, properties, ct)
                            .ConfigureAwait(false);

                        foreach (var header in streamContent.Headers)
                        {
                            response.Headers.Add(header.Key, header.Value.ToArray());
                        }

                        // Use the CopyToAsync function of the stream itself, because
                        // we're able to pass the cancellation token. This is a workaround
                        // for issue dotnet/corefx#9071 and fixes FubarDevelopment/WebDavServer#47.
                        await streamView.CopyToAsync(response.Body, SystemInfo.CopyBufferSize, ct)
                            .ConfigureAwait(false);
                    }
                }
                else
                {
                    // Multipart content
                    using (var multipart = new MultipartContent("byteranges"))
                    {
                        var index = 0;
                        foreach (var rangeItem in _rangeItems)
                        {
                            var streamView = views[index++];
                            var partContent = new StreamContent(streamView);
                            partContent.Headers.ContentRange = new ContentRangeHeaderValue(
                                rangeItem.From,
                                rangeItem.To,
                                _document.Length);
                            partContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                            partContent.Headers.ContentLength = rangeItem.Length;
                            multipart.Add(partContent);
                        }

                        await SetPropertiesToContentHeaderAsync(multipart, properties, ct)
                            .ConfigureAwait(false);

                        foreach (var header in multipart.Headers)
                        {
                            response.Headers.Add(header.Key, header.Value.ToArray());
                        }

                        // TODO: Workaround for issue dotnet/corefx#9071
                        await multipart.CopyToAsync(response.Body).ConfigureAwait(false);
                    }
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
                if (propValue.WasSet)
                    content.Headers.ContentLanguage.Add(propValue.Value);
            }
        }
    }
}
