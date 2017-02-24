// <copyright file="LockEventArgs.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// The event argument when a lock gets added to or removed from the lock manager
    /// </summary>
    public class LockEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockEventArgs"/> class.
        /// </summary>
        /// <param name="activeLock">The lock that got added to or remoted from the lock manager</param>
        public LockEventArgs(IActiveLock activeLock)
        {
            Lock = activeLock;
        }

        /// <summary>
        /// Gets the lock that got added to or remoted from the lock manager
        /// </summary>
        public IActiveLock Lock { get; }
    }
}
