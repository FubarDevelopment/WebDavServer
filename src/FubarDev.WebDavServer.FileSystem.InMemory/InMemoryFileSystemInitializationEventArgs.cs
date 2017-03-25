// <copyright file="InMemoryFileSystemInitializationEventArgs.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Security.Principal;

namespace FubarDev.WebDavServer.FileSystem.InMemory
{
    /// <summary>
    /// Event arguments for the file system initialization and update
    /// </summary>
    public class InMemoryFileSystemInitializationEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryFileSystemInitializationEventArgs"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system to be initialized or updated</param>
        /// <param name="principal">The principal for this file system</param>
        public InMemoryFileSystemInitializationEventArgs(IFileSystem fileSystem, IPrincipal principal)
        {
            FileSystem = fileSystem;
            Principal = principal;
        }

        /// <summary>
        /// Gets the file system
        /// </summary>
        public IFileSystem FileSystem { get; }

        /// <summary>
        /// Gets the principal
        /// </summary>
        public IPrincipal Principal { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the file system should be read-only
        /// </summary>
        public bool IsReadOnly { get; set; }
    }
}
