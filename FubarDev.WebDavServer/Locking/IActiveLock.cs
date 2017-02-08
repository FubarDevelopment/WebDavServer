// <copyright file="IActiveLock.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.Locking
{
    public interface IActiveLock : ILock
    {
        string StateToken { get; }

        DateTime Issued { get; }

        DateTime Expiration { get; }
    }
}
