// <copyright file="LockResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// The result of a locking operation.
    /// </summary>
    public class LockResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockResult"/> class.
        /// </summary>
        /// <param name="activeLock">The active lock when locking succeeded.</param>
        public LockResult(IActiveLock activeLock)
        {
            Lock = activeLock;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockResult"/> class.
        /// </summary>
        /// <param name="conflictingLocks">The collection of locks preventing locking the given destination.</param>
        public LockResult(LockStatus conflictingLocks)
        {
            ConflictingLocks = conflictingLocks;
        }

        /// <summary>
        /// Gets the active lock when locking succeeded.
        /// </summary>
        public IActiveLock? Lock { get; }

        /// <summary>
        /// Gets the collection of locks preventing locking the given destination.
        /// </summary>
        public LockStatus? ConflictingLocks { get; }
    }
}
