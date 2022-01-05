// <copyright file="DefaultImplicitLockFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// Default implementation of an implicit lock factory.
    /// </summary>
    public class DefaultImplicitLockFactory : IImplicitLockFactory
    {
        private readonly IFileSystem _rootFileSystem;
        private readonly IWebDavContextAccessor _webDavContextAccessor;
        private readonly ILockManager? _lockManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultImplicitLockFactory"/> class.
        /// </summary>
        /// <param name="rootFileSystem">The root file system.</param>
        /// <param name="webDavContextAccessor">The WebDAV context accessor.</param>
        public DefaultImplicitLockFactory(
            IFileSystem rootFileSystem,
            IWebDavContextAccessor webDavContextAccessor)
        {
            _rootFileSystem = rootFileSystem;
            _webDavContextAccessor = webDavContextAccessor;
            _lockManager = rootFileSystem.LockManager;
        }

        /// <inheritdoc />
        public Task<IImplicitLock> CreateAsync(ILock? lockRequirements, CancellationToken cancellationToken)
        {
            var useFakeLock = _lockManager == null
                || lockRequirements == null;
            if (useFakeLock)
            {
                return Task.FromResult<IImplicitLock>(new ImplicitLock(true));
            }

            return _lockManager!.LockImplicitAsync(
                _rootFileSystem,
                _webDavContextAccessor.WebDavContext.RequestHeaders.If,
                lockRequirements!,
                cancellationToken);
        }
    }
}
