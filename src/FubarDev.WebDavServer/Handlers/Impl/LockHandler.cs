// <copyright file="LockHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

using Timeout = FubarDev.WebDavServer.Model.Timeout;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    public class LockHandler : ILockHandler
    {
        [NotNull]
        private readonly IFileSystem _rootFileSystem;

        [NotNull]
        private readonly ILockManager _lockManager;

        [CanBeNull]
        private readonly ITimeoutPolicy _timeoutPolicy;

        [NotNull]
        private readonly IWebDavContext _webDavContext;

        public LockHandler(IWebDavContext context, IFileSystem rootFileSystem, ILockManager lockManager, ITimeoutPolicy timeoutPolicy = null)
        {
            _webDavContext = context;
            _rootFileSystem = rootFileSystem;
            _lockManager = lockManager;
            _timeoutPolicy = timeoutPolicy;
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; } = new[] { "LOCK" };

        /// <inheritdoc />
        public async Task<IWebDavResult> LockAsync(string path, lockinfo info, CancellationToken cancellationToken)
        {
            var owner = info.owner == null ? null : new XElement(WebDavXml.Dav + "owner", info.owner.Any.Cast<object>().ToArray());
            var recursive = (_webDavContext.RequestHeaders.Depth ?? Depth.Infinity) == Depth.Infinity;
            var accessType = LockAccessType.Write;
            var shareType = info.lockscope.ItemElementName == ItemChoiceType.exclusive
                ? LockShareMode.Exclusive
                : LockShareMode.Shared;
            var timeout = _timeoutPolicy?.SelectTimeout(
                              _webDavContext.RequestHeaders.Timeout?.Values ?? new[] { Timeout.Infinite })
                          ?? Timeout.Infinite;

            var l = new Lock(
                path,
                recursive,
                owner,
                accessType,
                shareType,
                timeout);

            var lockResult = await _lockManager.LockAsync(l, cancellationToken).ConfigureAwait(false);
            if (lockResult.ConflictingLocks != null)
            {
                // Lock cannot be acquired
            }

            var activeLock = lockResult.Lock;
            Debug.Assert(activeLock != null, "activeLock != null");
            try
            {
                var selectionResult = await _rootFileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);
                IEntry destination;
                if (selectionResult.IsMissing)
                {
                    var current = selectionResult.Collection;
                    var names = ImmutableList<string>.Empty.AddRange(selectionResult.MissingNames);
                    while (names.Count != 1)
                    {
                        var name = names[0];
                        names = names.RemoveAt(0);
                        var next = await current.CreateCollectionAsync(name, cancellationToken).ConfigureAwait(false);
                        current = next;
                    }

                    var docName = names.Single();
                    destination = await current.CreateDocumentAsync(docName, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    destination = selectionResult.TargetEntry;
                }
            }
            catch
            {
                await _lockManager
                    .ReleaseAsync(new Uri(activeLock.StateToken, UriKind.RelativeOrAbsolute), cancellationToken)
                    .ConfigureAwait(false);
                throw;
            }

            throw new NotImplementedException();
        }
    }
}
