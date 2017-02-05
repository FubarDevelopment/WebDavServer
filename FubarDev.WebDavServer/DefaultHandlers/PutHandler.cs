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

            Stream fileStream;
            if (selectionResult.ResultType == SelectionResultType.FoundDocument)
            {
                Debug.Assert(selectionResult.Document != null, "selectionResult.Document != null");
                fileStream = await selectionResult.Document.CreateAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                Debug.Assert(selectionResult.ResultType == SelectionResultType.MissingDocumentOrCollection, "selectionResult.ResultType == SelectionResultType.MissingDocumentOrCollection");
                Debug.Assert(selectionResult.MissingNames != null, "selectionResult.PathEntries != null");
                Debug.Assert(selectionResult.MissingNames.Count == 1, "selectionResult.MissingNames.Count == 1");
                Debug.Assert(selectionResult.Collection != null, "selectionResult.Collection != null");
                var newName = selectionResult.MissingNames.Single();
                var entry = await selectionResult.Collection.CreateDocumentAsync(newName, cancellationToken).ConfigureAwait(false);
                fileStream = await entry.CreateAsync(cancellationToken).ConfigureAwait(false);
            }

            using (fileStream)
            {
                await data.CopyToAsync(fileStream).ConfigureAwait(false);
            }

            if (selectionResult.ResultType == SelectionResultType.FoundDocument && selectionResult.Document?.FileSystem.PropertyStore != null)
            {
                await selectionResult.Document.FileSystem.PropertyStore.RemoveAsync(selectionResult.Document, cancellationToken).ConfigureAwait(false);
            }

            return new WebDavResult(selectionResult.ResultType != SelectionResultType.FoundDocument ? WebDavStatusCode.Created : WebDavStatusCode.OK);
        }
    }
}