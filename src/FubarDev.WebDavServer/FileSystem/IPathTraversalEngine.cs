// <copyright file="IPathTraversalEngine.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

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
        Task<SelectionResult> TraverseAsync(IFileSystem fileSystem, string? path, CancellationToken ct);

        /// <summary>
        /// Find the entry for a given path.
        /// </summary>
        /// <param name="currentCollection">The root collection.</param>
        /// <param name="path">The path to traverse.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The result for the path search.</returns>
        Task<SelectionResult> TraverseAsync(ICollection currentCollection, string? path, CancellationToken ct);
    }
}
