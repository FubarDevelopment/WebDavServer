// <copyright file="IActiveLock.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// An interface for the information about an active lock
    /// </summary>
    public interface IActiveLock : ILock
    {
        /// <summary>
        /// Gets the state token
        /// </summary>
        /// <remarks>
        /// This is always a valid URI (might be relative)
        /// </remarks>
        [NotNull]
        string StateToken { get; }

        /// <summary>
        /// Gets the timestamp when this lock was issued
        /// </summary>
        DateTime Issued { get; }

        /// <summary>
        /// Gets the timestamp when this lock expires
        /// </summary>
        /// <seealso cref="ILock.Timeout"/>
        DateTime Expiration { get; }
    }
}
