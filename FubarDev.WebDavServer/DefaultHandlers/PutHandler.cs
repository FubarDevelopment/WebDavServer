using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.DefaultHandlers
{
    public class PutHandler : IPutHandler
    {
        private readonly IFileSystem _fileSystem;

        public PutHandler(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public IEnumerable<string> HttpMethods { get; } = new[] { "PUT" };

        public async Task<IWebDavResult> PutAsync(string path, Stream data, CancellationToken cancellationToken)
        {
            var selectionResult = await _fileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);
            if (selectionResult.ResultType == SelectionResultType.MissingCollection)
                throw new WebDavException(WebDavStatusCodes.NotFound);
            if (selectionResult.ResultType == SelectionResultType.FoundCollection)
                throw new WebDavException(WebDavStatusCodes.MethodNotAllowed);

            Stream fileStream;
            if (selectionResult.ResultType == SelectionResultType.FoundDocument)
            {
                Debug.Assert(selectionResult.Document != null);
                fileStream = await selectionResult.Document.CreateAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                Debug.Assert(selectionResult.ResultType == SelectionResultType.MissingDocumentOrCollection);
                Debug.Assert(selectionResult.PathEntries != null, "selectionResult.PathEntries != null");
                Debug.Assert(selectionResult.PathEntries.Count == 1);
                var newName = selectionResult.PathEntries.Single();
                var entry = await selectionResult.Collection.CreateDocumentAsync(newName, cancellationToken).ConfigureAwait(false);
                fileStream = await entry.CreateAsync(cancellationToken).ConfigureAwait(false);
            }

            using (fileStream)
            {
                await data.CopyToAsync(fileStream).ConfigureAwait(false);
            }

            if (selectionResult.ResultType == SelectionResultType.FoundDocument && selectionResult.Document != null && _fileSystem.PropertyStore != null)
            {
                await _fileSystem.PropertyStore.RemoveAsync(selectionResult.Document, cancellationToken).ConfigureAwait(false);
            }

            return new WebDavResult(selectionResult.ResultType != SelectionResultType.FoundDocument ? WebDavStatusCodes.Created : WebDavStatusCodes.OK);
        }
    }
}