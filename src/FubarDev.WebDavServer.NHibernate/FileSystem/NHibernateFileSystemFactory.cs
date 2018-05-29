// <copyright file="SQLiteFileSystemFactory.cs" company="Fubar Development Junker">
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
    /// An implementation of <see cref="IFileSystemFactory"/> that provides file system storage in a SQLite database
    /// </summary>
    public class NHibernateFileSystemFactory : IFileSystemFactory
    {
        [NotNull] private readonly ISessionFactory _sessionFactory;

        [NotNull]
        private readonly IPathTraversalEngine _pathTraversalEngine;

        [CanBeNull]
        private readonly IPropertyStoreFactory _propertyStoreFactory;

        [CanBeNull]
        private readonly ILockManager _lockManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernateFileSystemFactory"/> class.
        /// </summary>
        /// <param name="sessionFactory">The NHibernate session factory</param>
        /// <param name="pathTraversalEngine">The engine to traverse paths</param>
        /// <param name="propertyStoreFactory">The store for dead properties</param>
        /// <param name="lockManager">The global lock manager</param>
        public NHibernateFileSystemFactory(
            [NotNull] ISessionFactory sessionFactory,
            [NotNull] IPathTraversalEngine pathTraversalEngine,
            [CanBeNull] IPropertyStoreFactory propertyStoreFactory = null,
            [CanBeNull] ILockManager lockManager = null)
        {
            _sessionFactory = sessionFactory;
            _pathTraversalEngine = pathTraversalEngine;
            _propertyStoreFactory = propertyStoreFactory;
            _lockManager = lockManager;
        }

        /// <inheritdoc />
        public virtual IFileSystem CreateFileSystem(ICollection mountPoint, IPrincipal principal)
        {
            return new NHibernateFileSystem(mountPoint, _sessionFactory.OpenSession(), _pathTraversalEngine, _lockManager, _propertyStoreFactory);
        }
    }
}
