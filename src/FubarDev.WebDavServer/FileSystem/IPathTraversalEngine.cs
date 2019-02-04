// <copyright file="IPathTraversalEngine.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem
{
    /// <summary>
    /// Interface for an implementation to find an entry for a given path
    /// </summary>
    public interface IPathTraversalEngine
    {
        /// <summary>
        /// Find the entry for a given path.
        /// </summary>
        /// <param name="fileSystem">The root file system.</param>
        /// <param name="path">The path to traverse.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The result for the path search.</returns>
        [NotNull]
        [ItemNotNull]
        Task<SelectionResult> TraverseAsync([NotNull] IFileSystem fileSystem, [CanBeNull] string path, CancellationToken ct);

        /// <summary>
        /// Find the entry for a given path.
        /// </summary>
        /// <param name="currentCollection">The root collection.</param>
        /// <param name="path">The path to traverse.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The result for the path search.</returns>
        [NotNull]
        [ItemNotNull]
        Task<SelectionResult> TraverseAsync([NotNull] ICollection currentCollection, [CanBeNull] string path, CancellationToken ct);
    }
}
