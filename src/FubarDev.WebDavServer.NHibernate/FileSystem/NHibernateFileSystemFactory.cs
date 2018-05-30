// <copyright file="NHibernateFileSystemFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Principal;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Props.Store;

using JetBrains.Annotations;

using NHibernate;

namespace FubarDev.WebDavServer.NHibernate.FileSystem
{
    /// <summary>
    /// An implementation of <see cref="IFileSystemFactory"/> that provides file system storage in a NHibernate-managed database
    /// </summary>
    public class NHibernateFileSystemFactory : IFileSystemFactory
    {
        [NotNull]
        private readonly ISession _session;

        [NotNull]
        private readonly IPathTraversalEngine _pathTraversalEngine;

        [CanBeNull]
        private readonly IPropertyStoreFactory _propertyStoreFactory;

        [CanBeNull]
        private readonly ILockManager _lockManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernateFileSystemFactory"/> class.
        /// </summary>
        /// <param name="session">The NHibernate session</param>
        /// <param name="pathTraversalEngine">The engine to traverse paths</param>
        /// <param name="propertyStoreFactory">The store for dead properties</param>
        /// <param name="lockManager">The global lock manager</param>
        public NHibernateFileSystemFactory(
            [NotNull] ISession session,
            [NotNull] IPathTraversalEngine pathTraversalEngine,
            [CanBeNull] IPropertyStoreFactory propertyStoreFactory = null,
            [CanBeNull] ILockManager lockManager = null)
        {
            _session = session;
            _pathTraversalEngine = pathTraversalEngine;
            _propertyStoreFactory = propertyStoreFactory;
            _lockManager = lockManager;
        }

        /// <inheritdoc />
        public virtual IFileSystem CreateFileSystem(ICollection mountPoint, IPrincipal principal)
        {
            return new NHibernateFileSystem(mountPoint, _session, _pathTraversalEngine, _lockManager, _propertyStoreFactory);
        }
    }
}
