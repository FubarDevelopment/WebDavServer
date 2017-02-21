// <copyright file="GetHeadHandler.cs" company="Fubar Development Junker">
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
using FubarDev.WebDavServer.Handlers.Impl.GetResults;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    public class GetHeadHandler : IGetHandler, IHeadHandler
    {
        [NotNull]
        private readonly IWebDavContext _context;

        public GetHeadHandler([NotNull] IFileSystem fileSystem, [NotNull] IWebDavContext context)
        {
            _context = context;
            FileSystem = fileSystem;
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; } = new[] { "GET", "HEAD" };

        [NotNull]
        public IFileSystem FileSystem { get; }

        /// <inheritdoc />
        public Task<IWebDavResult> GetAsync(string path, CancellationToken cancellationToken)
        {
            return HandleAsync(path, true, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> HeadAsync(string path, CancellationToken cancellationToken)
        {
            return HandleAsync(path, false, cancellationToken);
        }

        private async Task<IWebDavResult> HandleAsync([NotNull] string path, bool returnFile, CancellationToken cancellationToken)
        {
            var searchResult = await FileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);
            if (searchResult.IsMissing)
                throw new WebDavException(WebDavStatusCode.NotFound);

            var lockRequirements = new Lock(
                path,
                _context.RelativeRequestUrl.OriginalString,
                false,
                new XElement(WebDavXml.Dav + "owner", _context.User.Identity.Name),
                LockAccessType.Write,
                LockShareMode.Shared,
                TimeoutHeader.Infinite);
            var tempLock = await ImplicitLock.CreateAsync(FileSystem.LockManager, _context.RequestHeaders, lockRequirements, cancellationToken).ConfigureAwait(false);
            if (!tempLock.IsSuccessful)
            {
                var error = new error()
                {
                    ItemsElementName = new[] { ItemsChoiceType.locktokensubmitted, },
                    Items = new object[]
                    {
                        new errorLocktokensubmitted()
                        {
                            href = tempLock.ConflictingLocks.Select(x => x.Href).ToArray(),
                        },
                    },
                };

                return new WebDavResult<error>(WebDavStatusCode.Locked, error);
            }

            try
            {
                if (searchResult.ResultType == SelectionResultType.FoundCollection)
                {
                    if (returnFile)
                        throw new NotSupportedException();
                    Debug.Assert(searchResult.Collection != null, "searchResult.Collection != null");
                    return new WebDavCollectionResult(searchResult.Collection);
                }

                Debug.Assert(searchResult.Document != null, "searchResult.Document != null");

                var doc = searchResult.Document;
                var rangeHeader = _context.RequestHeaders.Range;
                if (rangeHeader != null)
                {
                    if (rangeHeader.Unit != "bytes")
                        throw new NotSupportedException();

                    var rangeItems = rangeHeader.Normalize(doc.Length);
                    if (rangeItems.Any(x => x.Length < 0 || x.To >= doc.Length))
                    {
                        return new WebDavResult(WebDavStatusCode.RequestedRangeNotSatisfiable)
                        {
                            Headers =
                            {
                                ["Content-Range"] = new[] { $"bytes */{doc.Length}" },
                            },
                        };
                    }

                    return new WebDavPartialDocumentResult(doc, returnFile, rangeItems);
                }

                return new WebDavFullDocumentResult(doc, returnFile);
            }
            finally
            {
                await tempLock.DisposeAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
