// <copyright file="MkColHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    public class MkColHandler : IMkColHandler
    {
        private readonly IFileSystem _rootFileSystem;

        public MkColHandler(IFileSystem rootFileSystem)
        {
            _rootFileSystem = rootFileSystem;
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; } = new[] { "MKCOL" };

        /// <inheritdoc />
        public async Task<IWebDavResult> MkColAsync(string path, CancellationToken cancellationToken)
        {
            var selectionResult = await _rootFileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);
            if (!selectionResult.IsMissing)
                throw new WebDavException(WebDavStatusCode.Forbidden);

            Debug.Assert(selectionResult.MissingNames != null, "selectionResult.PathEntries != null");
            if (selectionResult.MissingNames.Count != 1)
                throw new WebDavException(WebDavStatusCode.Conflict);

            var newName = selectionResult.MissingNames.Single();
            var collection = selectionResult.Collection;
            Debug.Assert(collection != null, "collection != null");
            try
            {
                await collection.CreateCollectionAsync(newName, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new WebDavException(WebDavStatusCode.Forbidden, ex);
            }

            return new WebDavResult(WebDavStatusCode.Created);
        }
    }
}
