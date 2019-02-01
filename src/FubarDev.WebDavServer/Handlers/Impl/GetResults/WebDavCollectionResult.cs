// <copyright file="WebDavCollectionResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Props.Live;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers.Impl.GetResults
{
    internal class WebDavCollectionResult : WebDavResult
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
            var lastWriteTimeProperty = _collection
                .GetLiveProperties().OfType<LastModifiedProperty>()
                .SingleOrDefault();
            if (lastWriteTimeProperty != null)
            {
                var lastWriteTimeUtc = await lastWriteTimeProperty.GetValueAsync(ct).ConfigureAwait(false);
                response.Headers["Last-Modified"] = new[] { lastWriteTimeUtc.ToString("R") };
            }
        }
    }
}
