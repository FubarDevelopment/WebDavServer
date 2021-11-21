// <copyright file="WebDavFullDocumentResult.cs" company="Fubar Development Junker">
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
    internal class WebDavFullDocumentResult : WebDavResult
    {
        private readonly IDocument _document;

        private readonly bool _returnFile;

        public WebDavFullDocumentResult(IDocument document, bool returnFile)
            : base(WebDavStatusCode.OK)
        {
            _document = document;
            _returnFile = returnFile;
        }

        public override async Task ExecuteResultAsync(IWebDavResponse response, CancellationToken ct)
        {
            await base.ExecuteResultAsync(response, ct).ConfigureAwait(false);

            if (_document.FileSystem.SupportsRangedRead)
            {
                response.Headers["Accept-Ranges"] = new[] { "bytes" };
            }

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

            using (var stream = await _document.OpenReadAsync(ct).ConfigureAwait(false))
            {
                using (var content = new StreamContent(stream))
                {
                    // I'm storing the headers in the content, because I'm too lazy to
                    // look up the header names and the formatting of its values.
                    await SetPropertiesToContentHeaderAsync(content, properties, ct).ConfigureAwait(false);

                    foreach (var header in content.Headers)
                    {
                        response.Headers.Add(header.Key, header.Value.ToArray());
                    }

                    // Use the CopyToAsync function of the stream itself, because
                    // we're able to pass the cancellation token. This is a workaround
                    // for issue dotnet/corefx#9071 and fixes FubarDevelopment/WebDavServer#47.
                    await stream.CopyToAsync(response.Body, 81920, ct)
                        .ConfigureAwait(false);
                }
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
                if (propValue.WasSet)
                {
                    content.Headers.ContentLanguage.Add(propValue.Value);
                }
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
}
