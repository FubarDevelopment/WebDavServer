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
        private readonly IWebDavContext _webDavContext;
        private readonly ILockManager? _lockManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultImplicitLockFactory"/> class.
        /// </summary>
        /// <param name="rootFileSystem">The root file system.</param>
        /// <param name="webDavContext">The current WebDAV context.</param>
        public DefaultImplicitLockFactory(
            IFileSystem rootFileSystem,
            IWebDavContext webDavContext)
        {
            _rootFileSystem = rootFileSystem;
            _webDavContext = webDavContext;
            _lockManager = rootFileSystem.LockManager;
        }

        /// <inheritdoc />
        public Task<IImplicitLock> CreateAsync(ILock? lockRequirements, CancellationToken cancellationToken)
        {
            var useFakeLock = _lockManager == null
                || lockRequirements == null
                || IsMostLikelyLockedByClient();
            if (useFakeLock)
            {
                return Task.FromResult<IImplicitLock>(new ImplicitLock(true));
            }

            return _lockManager!.LockImplicitAsync(
                _rootFileSystem,
                _webDavContext.RequestHeaders.If?.Lists,
                lockRequirements!,
                cancellationToken);
        }

        /// <summary>
        /// Returns whether the lock was (most likely) already acquired by the client.
        /// </summary>
        /// <returns><see langword="true"/> when the lock was most likely acquired by the client.</returns>
        protected virtual bool IsMostLikelyLockedByClient()
        {
            return IsMostLikelyLockedByWindows();
        }

        /// <summary>
        /// Returns whether the lock was (most likely) already acquired by the Windows WebDAV client.
        /// </summary>
        /// <returns><see langword="true"/> when the lock was most likely acquired - but only when the request was done by the Windows WebDAV client.</returns>
        protected bool IsMostLikelyLockedByWindows()
        {
            var client = _webDavContext.DetectedClient.UA?.Family;
            if (client == null || client != "Microsoft-WebDAV-MiniRedir")
            {
                return false;
            }

            try
            {
                var method = _webDavContext.RequestMethod.ToUpperInvariant();
                switch (method)
                {
                    case "PUT":
                    case "COPY":
                    case "MKCOL":
                    case "PROPPATCH":
                        return true;
                }
            }
            catch
            {
                // We don't have a request method.
                // Are we in a test environment?
            }

            return false;
        }
    }
}
