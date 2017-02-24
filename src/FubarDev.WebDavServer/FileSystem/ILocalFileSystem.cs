// <copyright file="ILocalFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.FileSystem
{
    /// <summary>
    /// Interface for a file system that's accessible using a file system path
    /// </summary>
    public interface ILocalFileSystem : IFileSystem
    {
        /// <summary>
        /// Gets the path to the root directory
        /// </summary>
        string RootDirectoryPath { get; }
    }
}
