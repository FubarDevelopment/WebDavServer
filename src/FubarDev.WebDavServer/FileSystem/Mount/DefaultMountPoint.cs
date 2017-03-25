// <copyright file="DefaultMountPoint.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem.Mount
{
    /// <summary>
    /// The default mount point implementation
    /// </summary>
    internal class DefaultMountPoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultMountPoint"/> class.
        /// </summary>
        /// <param name="source">The source path</param>
        /// <param name="destination">The destination file system</param>
        public DefaultMountPoint([NotNull] Uri source, [NotNull] IFileSystem destination)
        {
            Source = source;
            Destination = destination;
        }

        /// <summary>
        /// Gets the mount point source path
        /// </summary>
        [NotNull]
        public Uri Source { get; }

        /// <summary>
        /// Gets the mount point destination
        /// </summary>
        [NotNull]
        public IFileSystem Destination { get; }
    }
}
