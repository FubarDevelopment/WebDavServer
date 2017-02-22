// <copyright file="MkColHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.Utils;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    public class MkColHandler : IMkColHandler
    {
        private readonly IFileSystem _rootFileSystem;
        private readonly IWebDavContext _context;

        public MkColHandler(IFileSystem rootFileSystem, IWebDavContext context)
        {
            _rootFileSystem = rootFileSystem;
            _context = context;
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

            await _context.RequestHeaders
                .ValidateAsync(selectionResult.TargetEntry, cancellationToken).ConfigureAwait(false);

            var lockRequirements = new Lock(
                new Uri(path, UriKind.Relative),
                _context.RelativeRequestUrl,
                false,
                new XElement(WebDavXml.Dav + "owner", _context.User.Identity.Name),
                LockAccessType.Write,
                LockShareMode.Shared,
                TimeoutHeader.Infinite);
            var lockManager = _rootFileSystem.LockManager;
            var tempLock = lockManager == null
                ? new ImplicitLock(true)
                : await lockManager.LockImplicitAsync(_rootFileSystem, _context.RequestHeaders.If?.Lists, lockRequirements, cancellationToken)
                                   .ConfigureAwait(false);
            if (!tempLock.IsSuccessful)
                return tempLock.CreateErrorResponse();

            try
            {
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
            finally
            {
                await tempLock.DisposeAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
