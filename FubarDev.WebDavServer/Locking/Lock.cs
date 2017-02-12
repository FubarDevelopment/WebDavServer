// <copyright file="Lock.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// A generic implementation of the <see cref="ILock"/> interface.
    /// </summary>
    public class Lock : ILock
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Lock"/> class.
        /// </summary>
        /// <param name="path">The path (root-relative) this lock should be applied to</param>
        /// <param name="recursive">Must the lock be applied recursively to all children?</param>
        /// <param name="owner">The owner of the lock</param>
        /// <param name="accessType">The <see cref="LockAccessType"/> of the lock</param>
        /// <param name="shareMode">The <see cref="LockShareMode"/> of the lock</param>
        /// <param name="timeout">The lock timeout</param>
        public Lock(
            string path,
            bool recursive,
            XElement owner,
            LockAccessType accessType,
            LockShareMode shareMode,
            TimeSpan timeout)
            : this(
                path,
                recursive,
                owner,
                accessType.Id,
                shareMode.Id,
                timeout)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lock"/> class.
        /// </summary>
        /// <param name="path">The path (root-relative) this lock should be applied to</param>
        /// <param name="recursive">Must the lock be applied recursively to all children?</param>
        /// <param name="owner">The owner of the lock</param>
        /// <param name="accessType">The <see cref="LockAccessType"/> of the lock</param>
        /// <param name="shareMode">The <see cref="LockShareMode"/> of the lock</param>
        /// <param name="timeout">The lock timeout</param>
        protected Lock(
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

        /// <inheritdoc />
        public string Path { get; }

        /// <inheritdoc />
        public bool Recursive { get; }

        /// <summary>
        /// Gets the XML specifying the owner of the lock.
        /// </summary>
        public XElement Owner { get; }

        /// <inheritdoc />
        public string AccessType { get; }

        /// <inheritdoc />
        public string ShareMode { get; }

        /// <inheritdoc />
        public TimeSpan Timeout { get; }

        /// <inheritdoc />
        public XElement GetOwner()
            => Owner;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Path} [Recursive={Recursive}, AccessType={AccessType}, ShareMode={ShareMode}, Timeout={Timeout}, Owner={Owner}]";
        }
    }
}
