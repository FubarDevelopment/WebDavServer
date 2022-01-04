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

using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    /// <summary>
    /// Implements the <see cref="ILockManager"/> interface.
    /// </summary>
    public class LockHandler : ILockHandler
    {
        private readonly IFileSystem _rootFileSystem;
        private readonly ILockManager? _lockManager;
        private readonly ITimeoutPolicy? _timeoutPolicy;
        private readonly IWebDavContextAccessor _contextAccessor;
        private readonly bool _useAbsoluteHref = false;

        private readonly bool _encodeHref;

        /// <summary>
        /// Initializes a new instance of the <see cref="LockHandler"/> class.
        /// </summary>
        /// <param name="litmusCompatibilityOptions">The compatibility options for the litmus tests.</param>
        /// <param name="contextAccessor">The WebDAV request context accessor.</param>
        /// <param name="rootFileSystem">The root file system.</param>
        /// <param name="lockManager">The lock manager.</param>
        /// <param name="timeoutPolicy">The timeout policy for the selection of the <see cref="TimeoutHeader"/> value.</param>
        public LockHandler(
            IOptions<LitmusCompatibilityOptions> litmusCompatibilityOptions,
            IWebDavContextAccessor contextAccessor,
            IFileSystem rootFileSystem,
            ILockManager? lockManager = null,
            ITimeoutPolicy? timeoutPolicy = null)
        {
            _encodeHref = !litmusCompatibilityOptions.Value.DisableUrlEncodingOfResponseHref;
            _contextAccessor = contextAccessor;
            _rootFileSystem = rootFileSystem;
            _lockManager = lockManager;
            _timeoutPolicy = timeoutPolicy;
            HttpMethods = _lockManager == null ? Array.Empty<string>() : new[] { "LOCK" };
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; }

        /// <inheritdoc />
        public async Task<IWebDavResult> LockAsync(string path, lockinfo info, CancellationToken cancellationToken)
        {
            if (_lockManager == null)
            {
                throw new NotSupportedException();
            }

            var context = _contextAccessor.WebDavContext;
            var ownerHref = info.owner ?? context.User.Identity.GetOwnerHref();
            var recursive = (context.RequestHeaders.Depth ?? DepthHeader.Infinity) == DepthHeader.Infinity;
            var accessType = LockAccessType.Write;
            var shareType = info.lockscope.ItemElementName == ItemChoiceType.exclusive
                ? LockShareMode.Exclusive
                : LockShareMode.Shared;
            var timeout = _timeoutPolicy?.SelectTimeout(
                              context.RequestHeaders.Timeout?.Values ?? new[] { TimeoutHeader.Infinite })
                          ?? TimeoutHeader.Infinite;

            var href = GetHref(context, path);
            var l = new Lock(
                path,
                href,
                recursive,
                context.User.Identity.GetOwner(),
                ownerHref,
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
                    return new WebDavResult<error>(
                        WebDavStatusCode.Locked,
                        CreateError(context, lockResult.ConflictingLocks.GetLocks()));
                }

                var errorResponses = new List<response>();
                if (lockResult.ConflictingLocks.ChildLocks.Count != 0
                    || lockResult.ConflictingLocks.ReferenceLocks.Count != 0)
                {
                    errorResponses.Add(CreateErrorResponse(
                        context,
                        WebDavStatusCode.Forbidden,
                        lockResult.ConflictingLocks.ChildLocks.Concat(lockResult.ConflictingLocks.ReferenceLocks)));
                }

                if (lockResult.ConflictingLocks.ParentLocks.Count != 0)
                {
                    var errorResponse = CreateErrorResponse(
                        context,
                        WebDavStatusCode.Forbidden,
                        lockResult.ConflictingLocks.ChildLocks);
                    errorResponse.error = CreateError(context, Array.Empty<IActiveLock>());
                    errorResponses.Add(errorResponse);
                }

                errorResponses.Add(new response()
                {
                    href = GetHref(context, l.Path),
                    ItemsElementName = new[] { ItemsChoiceType2.status, },
                    Items = new object[]
                    {
                        new Status(context.RequestProtocol, WebDavStatusCode.FailedDependency).ToString(),
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
                    if (context.RequestHeaders.IfNoneMatch != null)
                    {
                        throw new WebDavException(WebDavStatusCode.PreconditionFailed);
                    }

                    if (selectionResult.MissingNames.Count > 1)
                    {
                        return new WebDavResult(WebDavStatusCode.Conflict);
                    }

                    var current = selectionResult.Collection;
                    var docName = selectionResult.MissingNames.Single();
                    await current.CreateDocumentAsync(docName, cancellationToken).ConfigureAwait(false);
                    statusCode = WebDavStatusCode.Created;
                }
                else
                {
                    await context
                        .RequestHeaders.ValidateAsync(selectionResult.TargetEntry, cancellationToken)
                        .ConfigureAwait(false);

                    statusCode = WebDavStatusCode.OK;
                }

                var activeLockXml = activeLock.ToXElement();
                var result = new prop()
                {
                    Any = new[] { new XElement(WebDavXml.Dav + "lockdiscovery", activeLockXml) },
                };
                var webDavResult = new WebDavResult<prop>(statusCode, result)
                {
                    Headers =
                    {
                        ["Lock-Token"] = new[]
                        {
                            new LockTokenHeader(new Uri(activeLock.StateToken)).ToString(),
                        },
                    },
                };
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

        /// <inheritdoc />
        public async Task<IWebDavResult> RefreshLockAsync(string path, IfHeader ifHeader, TimeoutHeader? timeoutHeader, CancellationToken cancellationToken)
        {
            if (_lockManager == null)
            {
                throw new NotSupportedException();
            }

            if (ifHeader.Lists.Any(x => x.Path.IsAbsoluteUri))
            {
                throw new InvalidOperationException("A Resource-Tag pointing to a different server or application isn't supported.");
            }

            var timeout = _timeoutPolicy?.SelectTimeout(
                              timeoutHeader?.Values ?? new[] { TimeoutHeader.Infinite })
                          ?? TimeoutHeader.Infinite;

            var result = await _lockManager.RefreshLockAsync(_rootFileSystem, ifHeader, timeout, cancellationToken).ConfigureAwait(false);
            if (result.ErrorResponse != null)
            {
                return new WebDavResult<error>(WebDavStatusCode.PreconditionFailed, result.ErrorResponse.error);
            }

            Debug.Assert(result.RefreshedLocks != null, "result.RefreshedLocks != null");
            var prop = new prop()
            {
                Any = result.RefreshedLocks.Select(x => new XElement(WebDavXml.Dav + "lockdiscovery", x.ToXElement())).ToArray(),
            };

            return new WebDavResult<prop>(WebDavStatusCode.OK, prop);
        }

        private error CreateError(IWebDavContext context, IEnumerable<IActiveLock> activeLocks)
        {
            return new error()
            {
                ItemsElementName = new[] { ItemsChoiceType.noconflictinglock, },
                Items = new object[]
                {
                    new errorNoconflictinglock()
                    {
                        href = GetHrefs(context, activeLocks),
                    },
                },
            };
        }

        private response CreateErrorResponse(
            IWebDavContext context,
            WebDavStatusCode statusCode,
            IEnumerable<IActiveLock> activeLocks)
        {
            var hrefs = GetHrefs(context, activeLocks);
            var choices = hrefs.Skip(1)
                .Select(x => Tuple.Create(ItemsChoiceType2.href, x))
                .ToList();
            choices.Add(Tuple.Create(
                ItemsChoiceType2.status,
                new Status(context.RequestProtocol, statusCode).ToString()));
            var response = new response()
            {
                href = hrefs.FirstOrDefault(),
                ItemsElementName = choices.Select(x => x.Item1).ToArray(),
                Items = choices.Select(x => x.Item2).Cast<object>().ToArray(),
            };

            return response;
        }

        private string[] GetHrefs(IWebDavContext context, IEnumerable<IActiveLock> activeLocks)
        {
            return activeLocks.Select(x => GetHref(context, x.Path)).ToArray();
        }

        private string GetHref(IWebDavContext context, string path)
        {
            var href = context.PublicControllerUrl.Append(path, true);
            if (!_useAbsoluteHref)
            {
                return "/" + context.PublicRootUrl.MakeRelativeUri(href).EncodeHref(_encodeHref);
            }

            return href.EncodeHref(_encodeHref);
        }
    }
}
