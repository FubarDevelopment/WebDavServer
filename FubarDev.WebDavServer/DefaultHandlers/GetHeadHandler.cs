using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.DefaultHandlers
{
    public class GetHandler : IGetHandler, IHeadHandler
    {
        public GetHandler(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        public IEnumerable<string> HttpMethods { get; } = new[] { "GET", "HEAD" };

        public IFileSystem FileSystem { get; }

        public Task<IWebDavResult> GetAsync(string path, CancellationToken cancellationToken)
        {
            return HandleAsync(path, true, cancellationToken);
        }

        public Task<IWebDavResult> HeadAsync(string path, CancellationToken cancellationToken)
        {
            return HandleAsync(path, false, cancellationToken);
        }

        private async Task<IWebDavResult> HandleAsync(string path, bool returnFile, CancellationToken cancellationToken)
        {
            var searchResult = await FileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);
            if (searchResult.IsMissing)
                throw new WebDavException(WebDavStatusCodes.NotFound);
            if (searchResult.ResultType != SelectionResultType.FoundDocument)
                throw new NotSupportedException();
            var doc = searchResult.Document;
            return new WebDavHandlerResult(FileSystem.PropertyStore, doc, returnFile);
        }

        private class WebDavHandlerResult : WebDavResult
        {
            [CanBeNull]
            private readonly IPropertyStore _propertyStore;

            [NotNull]
            private readonly IDocument _document;

            private readonly bool _returnFile;

            public WebDavHandlerResult([CanBeNull] IPropertyStore propertyStore, [NotNull] IDocument document, bool returnFile)
                : base(WebDavStatusCodes.OK)
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
                response.ContentType = "application/octet-stream";

                if (!_returnFile)
                    return;

                using (var stream = await _document.OpenReadAsync(ct).ConfigureAwait(false))
                {
                    await stream.CopyToAsync(response.Body, 65536, ct).ConfigureAwait(false);
                }
            }
        }
    }
}
