// <copyright file="IMountPointProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FubarDev.WebDavServer.FileSystem.Mount
{
    /// <summary>
    /// Base interface that allows querying the mount points
    /// </summary>
    public interface IMountPointProvider
    {
        /// <summary>
        /// Gets all mount points.
        /// </summary>
        IEnumerable<Uri> MountPoints { get; }

        /// <summary>
        /// Try to get a mount point for a given path.
        /// </summary>
        /// <param name="path">The path to get the destination file system for.</param>
        /// <param name="destination">The destination file system.</param>
        /// <returns><see langword="true"/> when there is a destination file system for a <paramref name="path"/>.</returns>
        bool TryGetMountPoint(Uri path, [NotNullWhen(true)] out IFileSystem? destination);
    }
}
