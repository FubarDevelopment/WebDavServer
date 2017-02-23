// <copyright file="IEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Props;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem
{
    /// <summary>
    /// An entry in the WebDAV file system
    /// </summary>
    public interface IEntry
    {
        /// <summary>
        /// Gets the name of the entry
        /// </summary>
        [NotNull]
        string Name { get; }

        /// <summary>
        /// Gets the root file system of the user
        /// </summary>
        [NotNull]
        IFileSystem RootFileSystem { get; }

        /// <summary>
        /// Gets the file system of this entry
        /// </summary>
        [NotNull]
        IFileSystem FileSystem { get; }

        /// <summary>
        /// Gets the collection that contains this entry
        /// </summary>
        /// <remarks>
        /// This property can be <code>null</code> when this entry is the root collection.
        /// </remarks>
        [CanBeNull]
        ICollection Parent { get; }

        /// <summary>
        /// Gets the path of the entry
        /// </summary>
        [NotNull]
        Uri Path { get; }

        /// <summary>
        /// Gets the last time this entry was modified
        /// </summary>
        DateTime LastWriteTimeUtc { get; }

        /// <summary>
        /// Gets the time this entry was created
        /// </summary>
        DateTime CreationTimeUtc { get; }

        /// <summary>
        /// Gets the properties assigned to this entry
        /// </summary>
        /// <param name="maxCost">The maximum cost the caller is allowed to pay to get the properties</param>
        /// <returns>The properties assigned to this entry</returns>
        [NotNull]
        IAsyncEnumerable<IUntypedReadableProperty> GetProperties(int? maxCost = null);

        /// <summary>
        /// Deletes this entry
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The result of the delete operation</returns>
        [NotNull]
        [ItemNotNull]
        Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken);
    }
}
