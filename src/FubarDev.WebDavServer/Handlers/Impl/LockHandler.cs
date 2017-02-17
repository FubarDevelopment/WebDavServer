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

using JetBrains.Annotations;

using TimeoutHeader = FubarDev.WebDavServer.Model.Headers.TimeoutHeader;

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
        private readonly IWebDavContext _context;

        private readonly bool _useAbsoluteHref = false;

        public LockHandler(IWebDavContext context, IFileSystem rootFileSystem, ILockManager lockManager, ITimeoutPolicy timeoutPolicy = null)
        {
            _context = context;
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
            var recursive = (_context.RequestHeaders.Depth ?? DepthHeader.Infinity) == DepthHeader.Infinity;
            var accessType = LockAccessType.Write;
            var shareType = info.lockscope.ItemElementName == ItemChoiceType.exclusive
                ? LockShareMode.Exclusive
                : LockShareMode.Shared;
            var timeout = _timeoutPolicy?.SelectTimeout(
                              _context.RequestHeaders.Timeout?.Values ?? new[] { TimeoutHeader.Infinite })
                          ?? TimeoutHeader.Infinite;

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
                    if (selectionResult.MissingNames.Count > 1)
                        return new WebDavResult(WebDavStatusCode.Conflict);

                    var current = selectionResult.Collection;
                    var docName = selectionResult.MissingNames.Single();
                    await current.CreateDocumentAsync(docName, cancellationToken).ConfigureAwait(false);
                    statusCode = WebDavStatusCode.Created;
                }
                else
                {
                    statusCode = WebDavStatusCode.OK;
                }

                var result = CreateActiveLockXml(activeLock);
                return new WebDavXmlResult(statusCode, result);
            }
            catch
            {
                await _lockManager
                    .ReleaseAsync(activeLock.Path, new Uri(activeLock.StateToken, UriKind.RelativeOrAbsolute), cancellationToken)
                    .ConfigureAwait(false);
                throw;
            }
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
                href = new Uri("/" + _context.RootUrl.MakeRelativeUri(href).OriginalString);
            return href.OriginalString;
        }

        private XElement CreateActiveLockXml(IActiveLock l)
        {
            var timeout = l.Timeout == TimeoutHeader.Infinite ? "Infinite" : $"Second-{l.Timeout.TotalSeconds:F0}";
            var depth = l.Recursive ? DepthHeader.Infinity : DepthHeader.Zero;
            var lockScope = LockShareMode.Parse(l.ShareMode);
            var lockType = LockAccessType.Parse(l.AccessType);
            var lockRoot = GetHref(l.Path);
            var owner = l.GetOwner();
            var result = new XElement(
                WebDavXml.Dav + "activelock",
                new XElement(
                    WebDavXml.Dav + "lockscope",
                    new XElement(lockScope.Name)),
                new XElement(
                    WebDavXml.Dav + "locktype",
                    new XElement(lockType.Name)),
                new XElement(
                    WebDavXml.Dav + "depth",
                    depth.Value));
            if (owner != null)
                result.Add(owner);
            result.Add(
                new XElement(
                    WebDavXml.Dav + "timeout",
                    timeout),
                new XElement(
                    WebDavXml.Dav + "locktoken",
                    new XElement(WebDavXml.Dav + "href", l.StateToken)),
                new XElement(
                    WebDavXml.Dav + "lockroot",
                    new XElement(WebDavXml.Dav + "href", lockRoot)));
            return result;
        }
    }
}
