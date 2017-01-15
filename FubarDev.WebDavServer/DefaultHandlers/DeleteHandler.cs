using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.DefaultHandlers
{
    public class DeleteHandler : IDeleteHandler
    {
        private readonly IFileSystem _rootFileSystem;

        private readonly IWebDavHost _host;

        public DeleteHandler(IFileSystem rootFileSystem, IWebDavHost host)
        {
            _rootFileSystem = rootFileSystem;
            _host = host;
        }

        public IEnumerable<string> HttpMethods { get; } = new[] { "DELETE" };

        public async Task<IWebDavResult> DeleteAsync(string path, CancellationToken cancellationToken)
        {
            var selectionResult = await _rootFileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);
            if (selectionResult.IsMissing)
                throw new WebDavException(WebDavStatusCodes.NotFound);

            var targetEntry = selectionResult.ResultType == SelectionResultType.FoundCollection ? (IEntry)selectionResult.Collection : selectionResult.Document;
            Debug.Assert(targetEntry != null);

            DeleteResult deleteResult;

            try
            {
                deleteResult = await targetEntry.DeleteAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                deleteResult = new DeleteResult(WebDavStatusCodes.Forbidden, targetEntry);
            }

            var result = new Multistatus()
            {
                Response = new[]
                {
                    new Response()
                    {
                        Href = new Uri(_host.BaseUrl, (deleteResult.FailedEntry ?? targetEntry).Path).ToString(),
                        ItemsElementName = new[] { ItemsChoiceType1.Status, },
                        Items = new object[] { $"{_host.RequestProtocol} {deleteResult.StatusCode} {deleteResult.StatusCode.GetReasonPhrase()}" }
                    }
                }
            };

            return new WebDavResult<Multistatus>(WebDavStatusCodes.MultiStatus, result);
        }
    }
}
