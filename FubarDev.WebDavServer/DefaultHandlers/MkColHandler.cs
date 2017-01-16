using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.DefaultHandlers
{
    public class MkColHandler : IMkColHandler
    {
        private readonly IFileSystem _rootFileSystem;

        public MkColHandler(IFileSystem rootFileSystem)
        {
            _rootFileSystem = rootFileSystem;
        }

        public IEnumerable<string> HttpMethods { get; } = new[] { "MKCOL" };

        public async Task<IWebDavResult> MkColAsync(string path, CancellationToken cancellationToken)
        {
            var selectionResult = await _rootFileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);
            if (!selectionResult.IsMissing)
                throw new WebDavException(WebDavStatusCodes.Forbidden);

            Debug.Assert(selectionResult.MissingNames != null, "selectionResult.PathEntries != null");
            if (selectionResult.MissingNames.Count != 1)
                throw new WebDavException(WebDavStatusCodes.Conflict);

            var newName = selectionResult.MissingNames.Single();
            var collection = selectionResult.Collection;
            Debug.Assert(collection != null, "collection != null");
            try
            {
                await collection.CreateCollectionAsync(newName, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new WebDavException(WebDavStatusCodes.Forbidden, ex);
            }

            return new WebDavResult(WebDavStatusCodes.Created);
        }
    }
}
