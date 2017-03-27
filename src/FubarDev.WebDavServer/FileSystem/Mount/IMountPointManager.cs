// <copyright file="IMountPointManager.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem.Mount
{
    /// <summary>
    /// Manager for mount points
    /// </summary>
    public interface IMountPointManager : IMountPointProvider
    {
        /// <summary>
        /// Mount a <paramref name="source"/> to a <paramref name="destination"/>
        /// </summary>
        /// <param name="source">The source path</param>
        /// <param name="destination">The destination file system</param>
        void Mount([NotNull] Uri source, [NotNull] IFileSystem destination);

        /// <summary>
        /// Removes a mount from the <paramref name="source"/>
        /// </summary>
        /// <param name="source">The source path</param>
        void Unmount([NotNull] Uri source);
    }
}
