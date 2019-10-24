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
        /// <param name="path">The file system path (root-relative) this lock should be applied to.</param>
        /// <param name="href">The href this lock should be applied to (might be relative or absolute).</param>
        /// <param name="recursive">Indicates whether the lock must be applied recursively to all children.</param>
        /// <param name="owner">The owner of the lock.</param>
        /// <param name="accessType">The <see cref="LockAccessType"/> of the lock.</param>
        /// <param name="shareMode">The <see cref="LockShareMode"/> of the lock.</param>
        /// <param name="timeout">The lock timeout.</param>
        public Lock(
            Uri path,
            Uri href,
            bool recursive,
            XElement? owner,
            LockAccessType accessType,
            LockShareMode shareMode,
            TimeSpan timeout)
            : this(
                path.OriginalString,
                href.OriginalString,
                recursive,
                owner,
                accessType.Name.LocalName,
                shareMode.Name.LocalName,
                timeout)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lock"/> class.
        /// </summary>
        /// <param name="path">The file system path (root-relative) this lock should be applied to.</param>
        /// <param name="href">The href this lock should be applied to (might be relative or absolute).</param>
        /// <param name="recursive">Indicates whether the lock must be applied recursively to all children.</param>
        /// <param name="owner">The owner of the lock.</param>
        /// <param name="accessType">The <see cref="LockAccessType"/> of the lock.</param>
        /// <param name="shareMode">The <see cref="LockShareMode"/> of the lock.</param>
        /// <param name="timeout">The lock timeout.</param>
        public Lock(
            string path,
            string href,
            bool recursive,
            XElement? owner,
            LockAccessType accessType,
            LockShareMode shareMode,
            TimeSpan timeout)
            : this(
                path,
                href,
                recursive,
                owner,
                accessType.Name.LocalName,
                shareMode.Name.LocalName,
                timeout)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lock"/> class.
        /// </summary>
        /// <param name="path">The file system path (root-relative) this lock should be applied to.</param>
        /// <param name="href">The href this lock should be applied to (might be relative or absolute).</param>
        /// <param name="recursive">Indicates whether the lock must be applied recursively to all children.</param>
        /// <param name="owner">The owner of the lock.</param>
        /// <param name="accessType">The <see cref="LockAccessType"/> of the lock.</param>
        /// <param name="shareMode">The <see cref="LockShareMode"/> of the lock.</param>
        /// <param name="timeout">The lock timeout.</param>
        protected Lock(
            string path,
            string href,
            bool recursive,
            XElement? owner,
            string accessType,
            string shareMode,
            TimeSpan timeout)
        {
            Path = path;
            Href = href;
            Recursive = recursive;
            Owner = owner;
            AccessType = accessType;
            ShareMode = shareMode;
            Timeout = timeout;
        }

        /// <inheritdoc />
        public string Path { get; }

        /// <inheritdoc />
        public string Href { get; }

        /// <inheritdoc />
        public bool Recursive { get; }

        /// <summary>
        /// Gets the XML specifying the owner of the lock.
        /// </summary>
        public XElement? Owner { get; }

        /// <inheritdoc />
        public string AccessType { get; }

        /// <inheritdoc />
        public string ShareMode { get; }

        /// <inheritdoc />
        public TimeSpan Timeout { get; }

        /// <inheritdoc />
        public XElement? GetOwner()
            => Owner;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Path={Path} [Href={Href} Recursive={Recursive}, AccessType={AccessType}, ShareMode={ShareMode}, Timeout={Timeout}, Owner={Owner}]";
        }
    }
}
