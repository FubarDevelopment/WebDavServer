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
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.Props;
using FubarDev.WebDavServer.Utils;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    /// <summary>
    /// Implementation of the <see cref="IMkColHandler"/> interface.
    /// </summary>
    public class MkColHandler : IMkColHandler
    {
        private readonly IFileSystem _rootFileSystem;

        private readonly IWebDavContextAccessor _contextAccessor;

        private readonly IImplicitLockFactory _implicitLockFactory;

        private readonly IEntryPropertyInitializer _entryPropertyInitializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MkColHandler"/> class.
        /// </summary>
        /// <param name="rootFileSystem">The root file system.</param>
        /// <param name="contextAccessor">The WebDAV request context.</param>
        /// <param name="implicitLockFactory">A factory to create implicit locks.</param>
        /// <param name="entryPropertyInitializer">The property initializer.</param>
        public MkColHandler(
            IFileSystem rootFileSystem,
            IWebDavContextAccessor contextAccessor,
            IImplicitLockFactory implicitLockFactory,
            IEntryPropertyInitializer entryPropertyInitializer)
        {
            _rootFileSystem = rootFileSystem;
            _contextAccessor = contextAccessor;
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
                // litmus: basic 11 (mkcol_again)
                throw new WebDavException(WebDavStatusCode.MethodNotAllowed);
            }

            Debug.Assert(selectionResult.MissingNames != null, "selectionResult.PathEntries != null");
            if (selectionResult.MissingNames.Count != 1)
            {
                throw new WebDavException(WebDavStatusCode.Conflict);
            }

            var context = _contextAccessor.WebDavContext;
            if (context.RequestHeaders.IfNoneMatch != null)
            {
                throw new WebDavException(WebDavStatusCode.PreconditionFailed);
            }

            var lockRequirements = new Lock(
                new Uri(path, UriKind.Relative),
                context.HrefUrl,
                false,
                context.User.Identity?.GetOwner(),
                context.User.Identity?.GetOwnerHref(),
                LockAccessType.Write,
                LockShareMode.Exclusive,
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
                                context,
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
