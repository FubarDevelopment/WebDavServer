// <copyright file="ActiveLock.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// A generic implementation of an active lock
    /// </summary>
    /// <remarks>
    /// The <see cref="ILockManager"/> implementation might use a different implementation
    /// of an <see cref="IActiveLock"/>.
    /// </remarks>
    public class ActiveLock : Lock, IActiveLock
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveLock"/> class.
        /// </summary>
        /// <param name="l">The lock to create this active lock from</param>
        /// <param name="issued">The date/time when this lock was issued</param>
        public ActiveLock([NotNull] ILock l, DateTime issued)
            : this(
                l.Path,
                l.Recursive,
                l.GetOwner(),
                LockAccessType.Parse(l.AccessType),
                LockShareMode.Parse(l.ShareMode),
                l.Timeout,
                issued)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveLock"/> class.
        /// </summary>
        /// <param name="l">The lock to create this active lock from</param>
        /// <param name="issued">The date/time when this lock was issued</param>
        /// <param name="timeout">Override the timeout from the original lock (to enforce rounding)</param>
        public ActiveLock([NotNull] ILock l, DateTime issued, TimeSpan timeout)
            : this(
                l.Path,
                l.Recursive,
                l.GetOwner(),
                LockAccessType.Parse(l.AccessType),
                LockShareMode.Parse(l.ShareMode),
                timeout,
                issued)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveLock"/> class.
        /// </summary>
        /// <param name="path">The path (root-relative) this lock should be applied to</param>
        /// <param name="recursive">Must the lock be applied recursively to all children?</param>
        /// <param name="owner">The owner of the lock</param>
        /// <param name="accessType">The <see cref="LockAccessType"/> of the lock</param>
        /// <param name="shareMode">The <see cref="LockShareMode"/> of the lock</param>
        /// <param name="timeout">The lock timeout</param>
        /// <param name="issued">The date/time when this lock was issued</param>
        public ActiveLock(
            [NotNull] string path,
            bool recursive,
            [CanBeNull] XElement owner,
            LockAccessType accessType,
            LockShareMode shareMode,
            TimeSpan timeout,
            DateTime issued)
            : base(
                path,
                recursive,
                owner,
                accessType.Name.LocalName,
                shareMode.Name.LocalName,
                timeout)
        {
            Issued = issued;
            Expiration = Issued + timeout;
            StateToken = $"urn:uuid:{Guid.NewGuid():D}";
        }

        /// <inheritdoc />
        public string StateToken { get; }

        /// <inheritdoc />
        public DateTime Issued { get; }

        /// <inheritdoc />
        public DateTime Expiration { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Path={Path} [Recursive={Recursive}, AccessType={AccessType}, ShareMode={ShareMode}, Timeout={Timeout}, Owner={Owner}, StateToken={StateToken}, Issued={Issued:O}, Expiration={Expiration:O}]";
        }
    }
}
