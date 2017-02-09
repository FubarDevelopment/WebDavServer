// <copyright file="ILock.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Locking
{
    public interface ILock
    {
        [NotNull]
        string Path { get; }

        bool Recursive { get; }

        [NotNull]
        string AccessType { get; }

        [NotNull]
        string ShareMode { get; }

        TimeSpan Timeout { get; }

        [NotNull]
        XElement GetOwner();
    }
}
