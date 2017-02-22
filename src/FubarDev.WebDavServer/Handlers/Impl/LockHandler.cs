// <copyright file="LockHandler.cs" company="Fubar Development Junker">
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

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    public class LockHandler : ILockHandler
    {
        [NotNull]
        private readonly IFileSystem _rootFileSystem;

        [CanBeNull]
        private readonly ILockManager _lockManager;

        [CanBeNull]
        private readonly ITimeoutPolicy _timeoutPolicy;

        [NotNull]
        private readonly IWebDavContext _context;

        private readonly bool _useAbsoluteHref = false;

        public LockHandler([NotNull] IWebDavContext context, [NotNull] IFileSystem rootFileSystem, ILockManager lockManager = null, ITimeoutPolicy timeoutPolicy = null)
        {
            _context = context;
            _rootFileSystem = rootFileSystem;
            _lockManager = lockManager;
            _timeoutPolicy = timeoutPolicy;
            HttpMethods = _lockManager == null ? new string[0] : new[] { "LOCK" };
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; }

        /// <inheritdoc />
        public async Task<IWebDavResult> LockAsync(string path, lockinfo info, CancellationToken cancellationToken)
        {
            if (_lockManager == null)
                throw new NotSupportedException();

            var owner = info.owner;
            var recursive = (_context.RequestHeaders.Depth ?? DepthHeader.Infinity) == DepthHeader.Infinity;
            var accessType = LockAccessType.Write;
            var shareType = info.lockscope.ItemElementName == ItemChoiceType.exclusive
                ? LockShareMode.Exclusive
                : LockShareMode.Shared;
            var timeout = _timeoutPolicy?.SelectTimeout(
                              _context.RequestHeaders.Timeout?.Values ?? new[] { TimeoutHeader.Infinite })
                          ?? TimeoutHeader.Infinite;

            var href = GetHref(path);
            var l = new Lock(
                path,
                href,
                recursive,
                owner,
                accessType,
                shareType,
                timeout);

            Debug.Assert(_lockManager != null, "_lockManager != null");
            var lockResult = await _lockManager.LockAsync(l, cancellationToken).ConfigureAwait(false);
            if (lockResult.ConflictingLocks != null)
            {
                // Lock cannot be acquired
                if (lockResult.ConflictingLocks.ChildLocks.Count == 0)
                {
                    return new WebDavResult<error>(WebDavStatusCode.Locked, CreateError(lockResult.ConflictingLocks.GetLocks()));
                }

                var errorResponses = new List<response>();
                if (lockResult.ConflictingLocks.ChildLocks.Count != 0
                    || lockResult.ConflictingLocks.ReferenceLocks.Count != 0)
                {
                    errorResponses.Add(CreateErrorResponse(
                        WebDavStatusCode.Forbidden,
                        lockResult.ConflictingLocks.ChildLocks.Concat(lockResult.ConflictingLocks.ReferenceLocks)));
                }

                if (lockResult.ConflictingLocks.ParentLocks.Count != 0)
                {
                    var errorResponse = CreateErrorResponse(
                        WebDavStatusCode.Forbidden,
                        lockResult.ConflictingLocks.ChildLocks);
                    errorResponse.error = CreateError(new IActiveLock[0]);
                    errorResponses.Add(errorResponse);
                }

                errorResponses.Add(new response()
                {
                    href = GetHref(l.Path),
                    ItemsElementName = new[] { ItemsChoiceType2.status, },
                    Items = new object[]
                    {
                        new Status(_context.RequestProtocol, WebDavStatusCode.FailedDependency).ToString(),
                    },
                });

                var multistatus = new multistatus()
                {
                    response = errorResponses.ToArray(),
                };

                return new WebDavResult<multistatus>(WebDavStatusCode.MultiStatus, multistatus);
            }

            var activeLock = lockResult.Lock;
            Debug.Assert(activeLock != null, "activeLock != null");
            try
            {
                var selectionResult = await _rootFileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);

                WebDavStatusCode statusCode;
                if (selectionResult.IsMissing)
                {
                    if (_context.RequestHeaders.IfNoneMatch != null)
                        throw new WebDavException(WebDavStatusCode.PreconditionFailed);

                    if (selectionResult.MissingNames.Count > 1)
                        return new WebDavResult(WebDavStatusCode.Conflict);

                    var current = selectionResult.Collection;
                    var docName = selectionResult.MissingNames.Single();
                    await current.CreateDocumentAsync(docName, cancellationToken).ConfigureAwait(false);
                    statusCode = WebDavStatusCode.Created;
                }
                else
                {
                    await _context
                        .RequestHeaders.ValidateAsync(selectionResult.TargetEntry, cancellationToken)
                        .ConfigureAwait(false);

                    statusCode = WebDavStatusCode.OK;
                }

                var activeLockXml = activeLock.ToXElement();
                var result = new prop()
                {
                    Any = new[] { new XElement(WebDavXml.Dav + "lockdiscovery", activeLockXml) },
                };
                var webDavResult = new WebDavResult<prop>(statusCode, result);
                webDavResult.Headers["Lock-Token"] = new[] { new LockTokenHeader(new Uri(activeLock.StateToken)).ToString() };
                return webDavResult;
            }
            catch
            {
                await _lockManager
                    .ReleaseAsync(activeLock.Path, new Uri(activeLock.StateToken, UriKind.RelativeOrAbsolute), cancellationToken)
                    .ConfigureAwait(false);
                throw;
            }
        }

        public async Task<IWebDavResult> RefreshLockAsync(string path, IfHeader ifHeader, TimeoutHeader timeoutHeader, CancellationToken cancellationToken)
        {
            if (_lockManager == null)
                throw new NotSupportedException();

            if (ifHeader.Lists.Any(x => x.Path.IsAbsoluteUri))
                throw new InvalidOperationException("A Resource-Tag pointing to a different server or application isn't supported.");

            var timeout = _timeoutPolicy?.SelectTimeout(
                              timeoutHeader?.Values ?? new[] { TimeoutHeader.Infinite })
                          ?? TimeoutHeader.Infinite;

            var result = await _lockManager.RefreshLockAsync(_rootFileSystem, ifHeader, timeout, cancellationToken).ConfigureAwait(false);
            if (result.ErrorResponse != null)
                return new WebDavResult<error>(WebDavStatusCode.PreconditionFailed, result.ErrorResponse.error);

            var prop = new prop()
            {
                Any = result.RefreshedLocks.Select(x => x.ToXElement()).ToArray(),
            };

            return new WebDavResult<prop>(WebDavStatusCode.OK, prop);
        }

        private error CreateError(IEnumerable<IActiveLock> activeLocks)
        {
            return new error()
            {
                ItemsElementName = new[] { ItemsChoiceType.noconflictinglock, },
                Items = new object[]
                {
                    new errorNoconflictinglock()
                    {
                        href = GetHrefs(activeLocks),
                    },
                },
            };
        }

        private response CreateErrorResponse(WebDavStatusCode statusCode, IEnumerable<IActiveLock> activeLocks)
        {
            var hrefs = GetHrefs(activeLocks);
            var choices = hrefs.Skip(1)
                .Select(x => Tuple.Create(ItemsChoiceType2.href, x))
                .ToList();
            choices.Add(Tuple.Create(
                ItemsChoiceType2.status,
                new Status(_context.RequestProtocol, statusCode).ToString()));
            var response = new response()
            {
                href = hrefs.FirstOrDefault(),
                ItemsElementName = choices.Select(x => x.Item1).ToArray(),
                Items = choices.Select(x => x.Item2).Cast<object>().ToArray(),
            };

            return response;
        }

        private string[] GetHrefs(IEnumerable<IActiveLock> activeLocks)
        {
            return activeLocks.Select(x => GetHref(x.Path)).ToArray();
        }

        private string GetHref(string path)
        {
            var href = _context.BaseUrl.Append(path, true);
            if (!_useAbsoluteHref)
                return "/" + _context.RootUrl.MakeRelativeUri(href).OriginalString;
            return href.OriginalString;
        }
    }
}
