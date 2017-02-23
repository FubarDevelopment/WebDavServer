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
    public class LockDiscoveryProperty : ILiveProperty
    {
        public static readonly XName PropertyName = WebDavXml.Dav + "lockdiscovery";

        [CanBeNull]
        private readonly ILockManager _lockManager;

        [NotNull]
        private readonly string _entryPath;

        public LockDiscoveryProperty([NotNull] IEntry entry)
        {
            _lockManager = entry.FileSystem.LockManager;
            _entryPath = entry.Path.OriginalString;
        }

        public XName Name { get; } = PropertyName;

        public IReadOnlyCollection<XName> AlternativeNames { get; } = new XName[0];

        public int Cost => _lockManager?.Cost ?? 0;

        public Task<XElement> GetXmlValueAsync(CancellationToken ct)
        {
            return GetXmlValueAsync(false, false, ct);
        }

        public async Task<XElement> GetXmlValueAsync(bool omitOwner, bool omitToken, CancellationToken ct)
        {
            if (_lockManager == null)
                return new XElement(Name);
            var affectedLocks = await _lockManager.GetAffectedLocksAsync(_entryPath, false, true, ct).ConfigureAwait(false);
            var lockElements = affectedLocks.Select(x => x.ToXElement(omitOwner: omitOwner, omitToken: omitToken)).Cast<object>().ToArray();
            return new XElement(Name, lockElements);
        }
    }
}
