// <copyright file="IFileSystemFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Principal;

namespace FubarDev.WebDavServer.FileSystem
{
    /// <summary>
    /// The file system factory
    /// </summary>
    public interface IFileSystemFactory
    {
        /// <summary>
        /// Gets the <see cref="IFileSystem"/> instance for the given <paramref name="principal"/>.
        /// </summary>
        /// <param name="mountPoint">The mount point where this file system should be included.</param>
        /// <param name="principal">The current principal to get the <see cref="IFileSystem"/> instance for.</param>
        /// <returns>The <see cref="IFileSystem"/> instance for the <paramref name="principal"/>.</returns>
        IFileSystem CreateFileSystem(ICollection? mountPoint, IPrincipal principal);
    }
}
