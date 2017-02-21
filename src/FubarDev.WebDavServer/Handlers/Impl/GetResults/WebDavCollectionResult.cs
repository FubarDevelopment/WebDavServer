// <copyright file="WebDavCollectionResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;

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
            response.Headers["Last-Modified"] = new[] { _collection.LastWriteTimeUtc.ToString("R") };
        }
    }
}
