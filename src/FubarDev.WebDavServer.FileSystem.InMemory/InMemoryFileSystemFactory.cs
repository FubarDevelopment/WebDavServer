﻿// <copyright file="InMemoryFileSystemFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Security.Principal;

using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Props.Store;
using FubarDev.WebDavServer.Utils;

namespace FubarDev.WebDavServer.FileSystem.InMemory
{
    /// <summary>
    /// An in-memory implementation of the <see cref="IFileSystemFactory"/>.
    /// </summary>
    public class InMemoryFileSystemFactory : IFileSystemFactory
    {
        private readonly Dictionary<FileSystemKey, InMemoryFileSystem> _fileSystems = new Dictionary<FileSystemKey, InMemoryFileSystem>();
        private readonly IPathTraversalEngine _pathTraversalEngine;
        private readonly ILockManager? _lockManager;
        private readonly IPropertyStoreFactory? _propertyStoreFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryFileSystemFactory"/> class.
        /// </summary>
        /// <param name="pathTraversalEngine">The engine to traverse paths.</param>
        /// <param name="lockManager">The global lock manager.</param>
        /// <param name="propertyStoreFactory">The store for dead properties.</param>
        public InMemoryFileSystemFactory(
            IPathTraversalEngine pathTraversalEngine,
            ILockManager? lockManager = null,
            IPropertyStoreFactory? propertyStoreFactory = null)
        {
            _pathTraversalEngine = pathTraversalEngine;
            _lockManager = lockManager;
            _propertyStoreFactory = propertyStoreFactory;
        }

        /// <inheritdoc />
        public virtual IFileSystem CreateFileSystem(ICollection? mountPoint, IPrincipal principal)
        {
            var userName = !principal.Identity.IsAnonymous()
                ? principal.Identity.Name
                : string.Empty;

            var key = new FileSystemKey(userName, mountPoint?.Path.OriginalString ?? string.Empty);
            if (!_fileSystems.TryGetValue(key, out var fileSystem))
            {
                fileSystem = new InMemoryFileSystem(mountPoint, _pathTraversalEngine, _lockManager, _propertyStoreFactory);
                _fileSystems.Add(key, fileSystem);
                InitializeFileSystem(mountPoint, principal, fileSystem);
            }
            else
            {
                UpdateFileSystem(mountPoint, principal, fileSystem);
            }

            return fileSystem;
        }

        /// <summary>
        /// Called when file system will be initialized.
        /// </summary>
        /// <param name="mountPoint">The mount point.</param>
        /// <param name="principal">The principal the file system was created for.</param>
        /// <param name="fileSystem">The created file system.</param>
        protected virtual void InitializeFileSystem(ICollection? mountPoint, IPrincipal principal, InMemoryFileSystem fileSystem)
        {
        }

        /// <summary>
        /// Called when the file system will be updated.
        /// </summary>
        /// <param name="mountPoint">The mount point.</param>
        /// <param name="principal">The principal the file system was created for.</param>
        /// <param name="fileSystem">The created file system.</param>
        protected virtual void UpdateFileSystem(ICollection? mountPoint, IPrincipal principal, InMemoryFileSystem fileSystem)
        {
        }

        private class FileSystemKey : IEquatable<FileSystemKey>
        {
            private static readonly IEqualityComparer<string> _comparer = StringComparer.OrdinalIgnoreCase;

            private readonly string _userName;

            private readonly string _mountPoint;

            public FileSystemKey(string userName, string mountPoint)
            {
                _userName = userName;
                _mountPoint = mountPoint;
            }

            public bool Equals(FileSystemKey other)
            {
                if (ReferenceEquals(null, other))
                {
                    return false;
                }

                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                return _comparer.Equals(_userName, other._userName) && _comparer.Equals(_mountPoint, other._mountPoint);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != GetType())
                {
                    return false;
                }

                return Equals((FileSystemKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((_userName != null ? _comparer.GetHashCode(_userName) : 0) * 397) ^ (_mountPoint != null ? _comparer.GetHashCode(_mountPoint) : 0);
                }
            }
        }
    }
}
