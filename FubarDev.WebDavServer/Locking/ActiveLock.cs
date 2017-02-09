// <copyright file="ActiveLock.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.Locking
{
    public class ActiveLock : IActiveLock
    {
        public ActiveLock(ILock l)
            : this(
                l.Path,
                l.Recursive,
                l.GetOwner(),
                l.AccessType,
                l.ShareMode,
                l.Timeout)
        {
        }

        public ActiveLock(
            string path,
            bool recursive,
            XElement owner,
            string accessType,
            string shareMode,
            TimeSpan timeout)
        {
            Path = path;
            Recursive = recursive;
            Owner = owner;
            AccessType = accessType;
            ShareMode = shareMode;
            Timeout = timeout;
            Issued = DateTime.UtcNow;
            Expiration = Issued + timeout;
            StateToken = $"urn:uuid:{Guid.NewGuid():D}";
        }

        public string Path { get; }

        public bool Recursive { get; }

        public XElement Owner { get; }

        public string AccessType { get; }

        public string ShareMode { get; }

        public TimeSpan Timeout { get; }

        public string StateToken { get; }

        public DateTime Issued { get; }

        public DateTime Expiration { get; }

        public XElement GetOwner()
            => Owner;
    }
}
