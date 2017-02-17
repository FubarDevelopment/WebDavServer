// <copyright file="UnlockHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    public class UnlockHandler : IUnlockHandler
    {
        private readonly IWebDavContext _context;
        private readonly ILockManager _lockManager;

        public UnlockHandler(IWebDavContext context, ILockManager lockManager)
        {
            _context = context;
            _lockManager = lockManager;
        }

        public IEnumerable<string> HttpMethods { get; } = new[] { "UNLOCK" };

        [SuppressMessage("ReSharper", "HeuristicUnreachableCode", Justification = "The header might be null, but technically it's not allowed")]
        public async Task<IWebDavResult> UnlockAsync(string path, LockTokenHeader stateToken, CancellationToken cancellationToken)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (stateToken == null)
                return new WebDavResult(WebDavStatusCode.BadRequest);

            var releaseStatus = await _lockManager.ReleaseAsync(path, stateToken.StateToken, cancellationToken).ConfigureAwait(false);
            if (releaseStatus != LockReleaseStatus.Success)
            {
                var href = new Uri(_context.BaseUrl, path);
                href = new Uri("/" + _context.RootUrl.MakeRelativeUri(href).OriginalString);
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
