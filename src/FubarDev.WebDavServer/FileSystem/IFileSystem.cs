// <copyright file="IFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Props.Store;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem
{
    /// <summary>
    /// The file system
    /// </summary>
    public interface IFileSystem
    {
        /// <summary>
        /// Gets the root collection.
        /// </summary>
        [NotNull]
        AsyncLazy<ICollection> Root { get; }

        /// <summary>
        /// Gets a value indicating whether the file system allows seeking and partial reading.
        /// </summary>
        bool SupportsRangedRead { get; }

        /// <summary>
        /// Gets the property store to be used for the file system.
        /// </summary>
        [CanBeNull]
        IPropertyStore PropertyStore { get; }

        /// <summary>
        /// Gets the global lock manager.
        /// </summary>
        [CanBeNull]
        ILockManager LockManager { get; }

        /// <summary>
        /// Finds an entry for a given path.
        /// </summary>
        /// <param name="path">The root-relative path.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The result of the search operation.</returns>
        [NotNull]
        [ItemNotNull]
        Task<SelectionResult> SelectAsync([NotNull] string path, CancellationToken ct);
    }
}
