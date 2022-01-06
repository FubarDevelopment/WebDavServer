// <copyright file="UnlockHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Models;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    /// <summary>
    /// The implementation of the <see cref="IUnlockHandler"/> interface.
    /// </summary>
    public class UnlockHandler : IUnlockHandler
    {
        private readonly IWebDavContextAccessor _contextAccessor;
        private readonly ILockManager? _lockManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnlockHandler"/> class.
        /// </summary>
        /// <param name="contextAccessor">The WebDAV request context accessor.</param>
        /// <param name="lockManager">The global lock manager.</param>
        public UnlockHandler(IWebDavContextAccessor contextAccessor, ILockManager? lockManager = null)
        {
            _contextAccessor = contextAccessor;
            _lockManager = lockManager;
            HttpMethods = _lockManager == null ? Array.Empty<string>() : new[] { "UNLOCK" };
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; }

        /// <inheritdoc />
        public async Task<IWebDavResult> UnlockAsync(string path, LockTokenHeader stateToken, CancellationToken cancellationToken)
        {
            if (_lockManager == null)
            {
                throw new NotSupportedException();
            }

            var releaseStatus = await _lockManager.ReleaseAsync(path, stateToken.StateToken, cancellationToken).ConfigureAwait(false);
            if (releaseStatus != LockReleaseStatus.Success)
            {
                var context = _contextAccessor.WebDavContext;
                var href = new Uri(context.PublicControllerUrl, path);
                href = new Uri("/" + context.PublicRootUrl.GetRelativeUrl(href).OriginalString);
                return new WebDavResult<error>(
                    WebDavStatusCode.Conflict,
                    new error()
                    {
                        ItemsElementName = new[] { ItemsChoiceType.noconflictinglock, },
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
