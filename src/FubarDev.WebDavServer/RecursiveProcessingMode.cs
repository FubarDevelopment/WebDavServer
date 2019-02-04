// <copyright file="RecursiveProcessingMode.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// Defines the mode that should be used to copy or move entries.
    /// </summary>
    public enum RecursiveProcessingMode
    {
        /// <summary>
        /// Prefer the fastest method (if possible)
        /// </summary>
        /// <remarks>
        /// This allows entries to be copied or moved within one file system.
        /// </remarks>
        PreferFastest,

        /// <summary>
        /// Prefer a slower copy/move operation that allows copying or moving between file systems.
        /// </summary>
        PreferCrossFileSystem,

        /// <summary>
        /// Prefer using a copy/move operation that allows copying between servers.
        /// </summary>
        PreferCrossServer,
    }
}
