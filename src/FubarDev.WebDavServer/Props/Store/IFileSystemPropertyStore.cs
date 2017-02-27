// <copyright file="IFileSystemPropertyStore.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Props.Store
{
    /// <summary>
    /// Interface for a property store that stores the properties on the local file system
    /// </summary>
    public interface IFileSystemPropertyStore : IPropertyStore
    {
        /// <summary>
        /// Gets or sets the root path of the property store
        /// </summary>
        string RootPath { get; set; }

        /// <summary>
        /// Determines whether the given <paramref name="entry"/> should be ignored when the client performs a PROPFIND
        /// </summary>
        /// <param name="entry">The entry that needs to be checked if it should be ignored</param>
        /// <returns><see langword="true"/> when the entry should be ignored</returns>
        bool IgnoreEntry(IEntry entry);
    }
}
