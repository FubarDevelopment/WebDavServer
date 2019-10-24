// <copyright file="IEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Props;

namespace FubarDev.WebDavServer.FileSystem
{
    /// <summary>
    /// An entry in the WebDAV file system
    /// </summary>
    public interface IEntry
    {
        /// <summary>
        /// Gets the name of the entry.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the file system of this entry.
        /// </summary>
        IFileSystem FileSystem { get; }

        /// <summary>
        /// Gets the collection that contains this entry.
        /// </summary>
        /// <remarks>
        /// This property can be <c>null</c> when this entry is the root collection.
        /// </remarks>
        ICollection? Parent { get; }

        /// <summary>
        /// Gets the path of the entry relative to the root file system.
        /// </summary>
        /// <remarks>
        /// The root file system may be different than the file system of this entry.
        /// </remarks>
        Uri Path { get; }

        /// <summary>
        /// Deletes this entry.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the delete operation.</returns>
        Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the live properties of this entry.
        /// </summary>
        /// <returns>The live properties of this entry.</returns>
        IEnumerable<IUntypedReadableProperty> GetLiveProperties();
    }
}
