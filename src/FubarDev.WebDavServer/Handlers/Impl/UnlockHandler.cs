// <copyright file="UnlockHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    /// <summary>
    /// The implementation of the <see cref="IUnlockHandler"/> interface
    /// </summary>
    public class UnlockHandler : IUnlockHandler
    {
        [NotNull]
        private readonly IWebDavContext _context;

        [CanBeNull]
        private readonly ILockManager _lockManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnlockHandler"/> class.
        /// </summary>
        /// <param name="context">The WebDAV request context</param>
        /// <param name="lockManager">The global lock manager</param>
        public UnlockHandler([NotNull] IWebDavContext context, [CanBeNull] ILockManager lockManager = null)
        {
            _context = context;
            _lockManager = lockManager;
            HttpMethods = _lockManager == null ? new string[0] : new[] { "UNLOCK" };
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; }

        /// <inheritdoc />
        public async Task<IWebDavResult> UnlockAsync(string path, LockTokenHeader stateToken, CancellationToken cancellationToken)
        {
            if (_lockManager == null)
                throw new NotSupportedException();

            var releaseStatus = await _lockManager.ReleaseAsync(path, stateToken.StateToken, cancellationToken).ConfigureAwait(false);
            if (releaseStatus != LockReleaseStatus.Success)
            {
                var href = new Uri(_context.PublicControllerUrl, path);
                href = new Uri("/" + _context.PublicRootUrl.MakeRelativeUri(href).OriginalString);
                return new WebDavResult<error>(
                    WebDavStatusCode.Conflict,
                    new error()
                    {
                        ItemsElementName = new[] { ItemsChoiceType.locktokenmatchesrequesturi, },
                        Items = new object[]
                        {
                            new errorNoconflictinglock()
                            {
                                href = new[] { href.OriginalString },
                            },
                        },
                    });
            }

            return new WebDavResult(WebDavStatusCode.NoContent);
        }
    }
}
