// <copyright file="ILock.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// Interface for a requested lock.
    /// </summary>
    public interface ILock
    {
        /// <summary>
        /// Gets the path the lock is for.
        /// </summary>
        [NotNull]
        string Path { get; }

        /// <summary>
        /// Gets the href the lock is for.
        /// </summary>
        [NotNull]
        string Href { get; }

        /// <summary>
        /// Gets a value indicating whether the lock must be applied recursively.
        /// </summary>
        bool Recursive { get; }

        /// <summary>
        /// Gets the access type of the lock.
        /// </summary>
        /// <seealso cref="LockAccessType"/>
        [NotNull]
        string AccessType { get; }

        /// <summary>
        /// Gets the share mode of the lock.
        /// </summary>
        /// <seealso cref="LockShareMode"/>
        [NotNull]
        string ShareMode { get; }

        /// <summary>
        /// Gets the timeout of the lock.
        /// </summary>
        /// <remarks>
        /// The lock is automatically released after the expiration of the timeout.
        /// </remarks>
        TimeSpan Timeout { get; }

        /// <summary>
        /// Gets the XML identifying the owner of the lock.
        /// </summary>
        /// <returns>
        /// The XML identifying the owner of the lock.
        /// </returns>
        [CanBeNull]
        XElement GetOwner();
    }
}
