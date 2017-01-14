using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.DefaultHandlers
{
    public class GetHandler : IGetHandler
    {
        public GetHandler(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        public IEnumerable<string> HttpMethods { get; } = new[] { "GET" };

        public IFileSystem FileSystem { get; }

        public async Task<IWebDavResult> HandleAsync(string path, CancellationToken cancellationToken)
        {
            var searchResult = await FileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);
            if (searchResult.IsMissing)
                throw new WebDavException(WebDavStatusCodes.NotFound);
            if (searchResult.ResultType != SelectionResultType.FoundDocument)
                throw new NotSupportedException();
            var doc = searchResult.Document;
            return new WebDavGetResult(doc);
        }

        private class WebDavGetResult : WebDavResult
        {
            private readonly IDocument _document;

            public WebDavGetResult(IDocument document)
                : base(WebDavStatusCodes.OK)
            {
                _document = document;
            }

            public override async Task ExecuteResultAsync(IWebDavResponse response, CancellationToken ct)
            {
                await base.ExecuteResultAsync(response, ct).ConfigureAwait(false);
                response.Headers["Last-Modified"] = new[] { _document.LastWriteTimeUtc.ToString("R") };
                response.ContentType = "application/octet-stream";
                using (var stream = await _document.OpenReadAsync(ct).ConfigureAwait(false))
                {
                    await stream.CopyToAsync(response.Body, 65536, ct).ConfigureAwait(false);
                }
            }
        }
    }
}
