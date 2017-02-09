// <copyright file="Lock.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.Locking
{
    public class Lock : ILock
    {
        public Lock(
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
        }

        public string Path { get; }

        public bool Recursive { get; }

        public XElement Owner { get; }

        public string AccessType { get; }

        public string ShareMode { get; }

        public TimeSpan Timeout { get; }

        public XElement GetOwner()
            => Owner;
    }
}
