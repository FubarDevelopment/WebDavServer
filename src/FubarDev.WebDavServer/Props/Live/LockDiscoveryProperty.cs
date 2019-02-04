// <copyright file="LockDiscoveryProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props.Live
{
    /// <summary>
    /// The <c>lockdiscovery</c> property.
    /// </summary>
    public class LockDiscoveryProperty : ILiveProperty
    {
        /// <summary>
        /// The XML property name.
        /// </summary>
        public static readonly XName PropertyName = WebDavXml.Dav + "lockdiscovery";

        [CanBeNull]
        private readonly ILockManager _lockManager;

        [NotNull]
        private readonly string _entryPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="LockDiscoveryProperty"/> class.
        /// </summary>
        /// <param name="entry">The file system entry.</param>
        public LockDiscoveryProperty([NotNull] IEntry entry)
        {
            _lockManager = entry.FileSystem.LockManager;
            _entryPath = entry.Path.OriginalString;
        }

        /// <inheritdoc />
        public XName Name { get; } = PropertyName;

        /// <inheritdoc />
        public string Language { get; } = null;

        /// <inheritdoc />
        public IReadOnlyCollection<XName> AlternativeNames { get; } = new XName[0];

        /// <inheritdoc />
        public int Cost => _lockManager?.Cost ?? 0;

        /// <inheritdoc />
        public Task<bool> IsValidAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<XElement> GetXmlValueAsync(CancellationToken ct)
        {
            return GetXmlValueAsync(false, true, ct);
        }

        /// <summary>
        /// Get the XML value asynchronously.
        /// </summary>
        /// <param name="omitOwner">Indicates whether we want to omit the owner.</param>
        /// <param name="omitToken">Indicates whether we want to omit the lock token</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The XML value.</returns>
        public async Task<XElement> GetXmlValueAsync(bool omitOwner, bool omitToken, CancellationToken ct)
        {
            if (_lockManager == null)
            {
                return new XElement(Name);
            }

            var affectedLocks = await _lockManager.GetAffectedLocksAsync(_entryPath, false, true, ct).ConfigureAwait(false);
            var lockElements = affectedLocks.Select(x => x.ToXElement(omitOwner: omitOwner, omitToken: omitToken)).Cast<object>().ToArray();
            return new XElement(Name, lockElements);
        }
    }
}
