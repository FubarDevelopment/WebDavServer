// <copyright file="DeleteHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    public class DeleteHandler : IDeleteHandler
    {
        private readonly IFileSystem _rootFileSystem;

        private readonly IWebDavContext _host;

        public DeleteHandler(IFileSystem rootFileSystem, IWebDavContext host)
        {
            _rootFileSystem = rootFileSystem;
            _host = host;
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; } = new[] { "DELETE" };

        /// <inheritdoc />
        public async Task<IWebDavResult> DeleteAsync(string path, CancellationToken cancellationToken)
        {
            var selectionResult = await _rootFileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);
            if (selectionResult.IsMissing)
                throw new WebDavException(WebDavStatusCode.NotFound);

            var targetEntry = selectionResult.ResultType == SelectionResultType.FoundCollection ? (IEntry)selectionResult.Collection : selectionResult.Document;
            Debug.Assert(targetEntry != null, "targetEntry != null");

            DeleteResult deleteResult;

            try
            {
                deleteResult = await targetEntry.DeleteAsync(cancellationToken).ConfigureAwait(false);
                if (targetEntry.FileSystem.PropertyStore != null)
                {
                    // Remove dead properties (if there are any)
                    await targetEntry.FileSystem.PropertyStore.RemoveAsync(targetEntry, cancellationToken).ConfigureAwait(false);
                }
            }
            catch
            {
                deleteResult = new DeleteResult(WebDavStatusCode.Forbidden, targetEntry);
            }

            var result = new Multistatus()
            {
                Response = new[]
                {
                    new Response()
                    {
                        Href = _host.BaseUrl.Append((deleteResult.FailedEntry ?? targetEntry).Path).OriginalString,
                        ItemsElementName = new[] { ItemsChoiceType2.Status, },
                        Items = new object[] { new Status(_host.RequestProtocol, deleteResult.StatusCode).ToString() },
                    },
                },
            };

            return new WebDavResult<Multistatus>(WebDavStatusCode.MultiStatus, result);
        }
    }
}
