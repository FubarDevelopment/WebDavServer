// <copyright file="WebDavCollectionResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props.Live;
using FubarDev.WebDavServer.Utils;

namespace FubarDev.WebDavServer.Handlers.Impl.GetResults
{
    internal class WebDavCollectionResult : WebDavResult, IDisposable
    {
        private readonly ICollection _collection;

        public WebDavCollectionResult(ICollection collection)
            : base(WebDavStatusCode.OK)
        {
            _collection = collection;
        }

        public Stream? ResponseStream { get; init; }

        public override async Task ExecuteResultAsync(IWebDavResponse response, CancellationToken ct)
        {
            await base.ExecuteResultAsync(response, ct).ConfigureAwait(false);

            var lastWriteTimeProperty = _collection
                .GetLiveProperties().OfType<LastModifiedProperty>()
                .SingleOrDefault();
            if (lastWriteTimeProperty != null)
            {
                var lastWriteTimeUtc = await lastWriteTimeProperty.GetValueAsync(ct).ConfigureAwait(false);
                response.Headers["Last-Modified"] = new[] { lastWriteTimeUtc.ToString("R") };
            }

            if (ResponseStream != null)
            {
                try
                {
                    await ResponseStream.CopyToAsync(response.Body, SystemInfo.CopyBufferSize, ct)
                        .ConfigureAwait(false);
                }
                finally
                {
                    ResponseStream.Position = 0;
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            ResponseStream?.Dispose();
        }
    }
}
