// <copyright file="PutHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; } = new[] { "PUT" };

        /// <inheritdoc />
        public async Task<IWebDavResult> PutAsync(string path, Stream data, CancellationToken cancellationToken)
        {
            var selectionResult = await _fileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);
            if (selectionResult.ResultType == SelectionResultType.MissingCollection)
                throw new WebDavException(WebDavStatusCode.NotFound);
            if (selectionResult.ResultType == SelectionResultType.FoundCollection)
                throw new WebDavException(WebDavStatusCode.MethodNotAllowed);

            IDocument document;
            if (selectionResult.ResultType == SelectionResultType.FoundDocument)
            {
                Debug.Assert(selectionResult.Document != null, "selectionResult.Document != null");
                document = selectionResult.Document;
            }
            else
            {
                Debug.Assert(selectionResult.ResultType == SelectionResultType.MissingDocumentOrCollection, "selectionResult.ResultType == SelectionResultType.MissingDocumentOrCollection");
                Debug.Assert(selectionResult.MissingNames != null, "selectionResult.PathEntries != null");
                Debug.Assert(selectionResult.MissingNames.Count == 1, "selectionResult.MissingNames.Count == 1");
                Debug.Assert(selectionResult.Collection != null, "selectionResult.Collection != null");
                var newName = selectionResult.MissingNames.Single();
                document = await selectionResult.Collection.CreateDocumentAsync(newName, cancellationToken).ConfigureAwait(false);
            }

            using (var fileStream = await document.CreateAsync(cancellationToken).ConfigureAwait(false))
            {
                await data.CopyToAsync(fileStream).ConfigureAwait(false);
            }

            var docPropertyStore = document.FileSystem.PropertyStore;
            if (docPropertyStore != null)
            {
                await docPropertyStore.UpdateETagAsync(document, cancellationToken).ConfigureAwait(false);

                if (selectionResult.ResultType == SelectionResultType.FoundDocument)
                {
                    await docPropertyStore.RemoveAsync(selectionResult.Document, cancellationToken).ConfigureAwait(false);
                }
            }

            var parent = document.Parent;
            Debug.Assert(parent != null, "parent != null");
            var parentPropStore = parent.FileSystem.PropertyStore;
            if (parentPropStore != null)
            {
                await parentPropStore.UpdateETagAsync(parent, cancellationToken).ConfigureAwait(false);
            }

            return new WebDavResult(selectionResult.ResultType != SelectionResultType.FoundDocument ? WebDavStatusCode.Created : WebDavStatusCode.OK);
        }
    }
}