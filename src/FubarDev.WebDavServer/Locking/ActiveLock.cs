// <copyright file="ActiveLock.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model.Headers;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// A generic implementation of an active lock
    /// </summary>
    /// <remarks>
    /// The <see cref="ILockManager"/> implementation might use a different implementation
    /// of an <see cref="IActiveLock"/>.
    /// </remarks>
    internal class ActiveLock : Lock, IActiveLock
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveLock"/> class.
        /// </summary>
        /// <param name="l">The lock to create this active lock from.</param>
        /// <param name="issued">The date/time when this lock was issued.</param>
        internal ActiveLock(ILock l, DateTime issued)
            : this(
                l.Path,
                l.Href,
                l.Recursive,
                l.Owner,
                l.GetOwnerHref(),
                LockAccessType.Parse(l.AccessType),
                LockShareMode.Parse(l.ShareMode),
                l.Timeout,
                issued,
                null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveLock"/> class.
        /// </summary>
        /// <param name="l">The lock to create this active lock from.</param>
        /// <param name="issued">The date/time when this lock was issued.</param>
        /// <param name="timeout">Override the timeout from the original lock (to enforce rounding).</param>
        internal ActiveLock(ILock l, DateTime issued, TimeSpan timeout)
            : this(
                l.Path,
                l.Href,
                l.Recursive,
                l.Owner,
                l.GetOwnerHref(),
                LockAccessType.Parse(l.AccessType),
                LockShareMode.Parse(l.ShareMode),
                timeout,
                issued,
                null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveLock"/> class.
        /// </summary>
        /// <param name="path">The file system path (root-relative) this lock should be applied to.</param>
        /// <param name="href">The href this lock should be applied to (might be relative or absolute).</param>
        /// <param name="recursive">Indicates whether the lock must be applied recursively to all children.</param>
        /// <param name="owner">The owner of the lock.</param>
        /// <param name="ownerHref">The owner href of the lock.</param>
        /// <param name="accessType">The <see cref="LockAccessType"/> of the lock.</param>
        /// <param name="shareMode">The <see cref="LockShareMode"/> of the lock.</param>
        /// <param name="timeout">The lock timeout.</param>
        /// <param name="issued">The date/time when this lock was issued.</param>
        /// <param name="lastRefresh">The date/time of the last refresh.</param>
        internal ActiveLock(
            string path,
            string href,
            bool recursive,
            string? owner,
            XElement? ownerHref,
            LockAccessType accessType,
            LockShareMode shareMode,
            TimeSpan timeout,
            DateTime issued,
            DateTime? lastRefresh)
            : base(
                path,
                href,
                recursive,
                owner,
                ownerHref,
                accessType.Name.LocalName,
                shareMode.Name.LocalName,
                timeout)
        {
            Issued = issued;
            LastRefresh = lastRefresh;
            Expiration = timeout == TimeoutHeader.Infinite ? DateTime.MaxValue : (lastRefresh ?? issued) + timeout;
            StateToken = $"urn:uuid:{Guid.NewGuid():D}";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveLock"/> class.
        /// </summary>
        /// <param name="path">The file system path (root-relative) this lock should be applied to.</param>
        /// <param name="href">The href this lock should be applied to (might be relative or absolute).</param>
        /// <param name="recursive">Indicates whether the lock must be applied recursively to all children.</param>
        /// <param name="owner">The owner of the lock.</param>
        /// <param name="ownerHref">The owner href of the lock.</param>
        /// <param name="accessType">The <see cref="LockAccessType"/> of the lock.</param>
        /// <param name="shareMode">The <see cref="LockShareMode"/> of the lock.</param>
        /// <param name="timeout">The lock timeout.</param>
        /// <param name="issued">The date/time when this lock was issued.</param>
        /// <param name="lastRefresh">The date/time of the last refresh.</param>
        /// <param name="stateToken">The state token.</param>
        internal ActiveLock(
            string path,
            string href,
            bool recursive,
            string? owner,
            XElement? ownerHref,
            LockAccessType accessType,
            LockShareMode shareMode,
            TimeSpan timeout,
            DateTime issued,
            DateTime? lastRefresh,
            string stateToken)
            : base(
                path,
                href,
                recursive,
                owner,
                ownerHref,
                accessType.Name.LocalName,
                shareMode.Name.LocalName,
                timeout)
        {
            Issued = issued;
            LastRefresh = lastRefresh;
            Expiration = timeout == TimeoutHeader.Infinite ? DateTime.MaxValue : (lastRefresh ?? issued) + timeout;
            StateToken = stateToken;
        }

        /// <inheritdoc />
        public string StateToken { get; }

        /// <inheritdoc />
        public DateTime Issued { get; }

        /// <inheritdoc />
        public DateTime? LastRefresh { get; }

        /// <inheritdoc />
        public DateTime Expiration { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Path={Path} [Href={Href}, Recursive={Recursive}, AccessType={AccessType}, ShareMode={ShareMode}, Timeout={Timeout}, Owner={Owner}, StateToken={StateToken}, Issued={Issued:O}, LastRefresh={LastRefresh:O}, Expiration={Expiration:O}]";
        }
    }
}
