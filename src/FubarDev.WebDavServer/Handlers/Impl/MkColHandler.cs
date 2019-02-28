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
using FubarDev.WebDavServer.Props;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    /// <summary>
    /// Implementation of the <see cref="IMkColHandler"/> interface.
    /// </summary>
    public class MkColHandler : IMkColHandler
    {
        [NotNull]
        private readonly IFileSystem _rootFileSystem;

        [NotNull]
        private readonly IWebDavContext _context;

        [NotNull]
        private readonly IImplicitLockFactory _implicitLockFactory;

        [NotNull]
        private readonly IEntryPropertyInitializer _entryPropertyInitializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MkColHandler"/> class.
        /// </summary>
        /// <param name="rootFileSystem">The root file system.</param>
        /// <param name="context">The WebDAV request context.</param>
        /// <param name="implicitLockFactory">A factory to create implicit locks.</param>
        /// <param name="entryPropertyInitializer">The property initializer.</param>
        public MkColHandler(
            [NotNull] IFileSystem rootFileSystem,
            [NotNull] IWebDavContext context,
            [NotNull] IImplicitLockFactory implicitLockFactory,
            [NotNull] IEntryPropertyInitializer entryPropertyInitializer)
        {
            _rootFileSystem = rootFileSystem;
            _context = context;
            _implicitLockFactory = implicitLockFactory;
            _entryPropertyInitializer = entryPropertyInitializer;
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; } = new[] { "MKCOL" };

        /// <inheritdoc />
        public async Task<IWebDavResult> MkColAsync(string path, CancellationToken cancellationToken)
        {
            var selectionResult = await _rootFileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);
            if (!selectionResult.IsMissing)
            {
                throw new WebDavException(WebDavStatusCode.Forbidden);
            }

            Debug.Assert(selectionResult.MissingNames != null, "selectionResult.PathEntries != null");
            if (selectionResult.MissingNames.Count != 1)
            {
                throw new WebDavException(WebDavStatusCode.Conflict);
            }

            if (_context.RequestHeaders.IfNoneMatch != null)
            {
                throw new WebDavException(WebDavStatusCode.PreconditionFailed);
            }

            var lockRequirements = new Lock(
                new Uri(path, UriKind.Relative),
                _context.PublicRelativeRequestUrl,
                false,
                new XElement(WebDavXml.Dav + "owner", _context.User.Identity.Name),
                LockAccessType.Write,
                LockShareMode.Shared,
                TimeoutHeader.Infinite);
            var tempLock = await _implicitLockFactory.CreateAsync(lockRequirements, cancellationToken).ConfigureAwait(false);
            if (!tempLock.IsSuccessful)
            {
                return tempLock.CreateErrorResponse();
            }

            try
            {
                var newName = selectionResult.MissingNames.Single();
                var collection = selectionResult.Collection;
                Debug.Assert(collection != null, "collection != null");
                try
                {
                    var newCollection = await collection.CreateCollectionAsync(newName, cancellationToken)
                        .ConfigureAwait(false);
                    if (newCollection.FileSystem.PropertyStore != null)
                    {
                        await _entryPropertyInitializer.CreatePropertiesAsync(
                                newCollection,
                                newCollection.FileSystem.PropertyStore,
                                _context,
                                cancellationToken)
                            .ConfigureAwait(false);
                    }
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
