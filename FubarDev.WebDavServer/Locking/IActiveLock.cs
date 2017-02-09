// <copyright file="IActiveLock.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Locking
{
    public interface IActiveLock : ILock
    {
        [NotNull]
        string StateToken { get; }

        DateTime Issued { get; }

        DateTime Expiration { get; }
    }
}
