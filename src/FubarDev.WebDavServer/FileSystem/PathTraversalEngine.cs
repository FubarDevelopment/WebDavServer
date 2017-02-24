// <copyright file="PathTraversalEngine.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.FileSystem
{
    /// <summary>
    /// Helper class to find an entry for a given path
    /// </summary>
    public class PathTraversalEngine
    {
        [CanBeNull]
        private readonly ILogger<PathTraversalEngine> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathTraversalEngine"/> class.
        /// </summary>
        /// <param name="logger">The logger</param>
        public PathTraversalEngine(ILogger<PathTraversalEngine> logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Find the entry for a given path
        /// </summary>
        /// <param name="fileSystem">The root file system</param>
        /// <param name="path">The path to traverse</param>
        /// <param name="ct">The cancellation token</param>
        /// <returns>The result for the path search</returns>
        [NotNull]
        [ItemNotNull]
        public Task<SelectionResult> TraverseAsync([NotNull] IFileSystem fileSystem, [CanBeNull] string path, CancellationToken ct)
        {
            return TraverseAsync(fileSystem, SplitPath(path ?? string.Empty), ct);
        }

        /// <summary>
        /// Find the entry for a given path
        /// </summary>
        /// <param name="currentCollection">The root collection</param>
        /// <param name="path">The path to traverse</param>
        /// <param name="ct">The cancellation token</param>
        /// <returns>The result for the path search</returns>
        [NotNull]
        [ItemNotNull]
        public Task<SelectionResult> TraverseAsync([NotNull] ICollection currentCollection, [CanBeNull] string path, CancellationToken ct)
        {
            return TraverseAsync(currentCollection, SplitPath(path ?? string.Empty), ct);
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<string> SplitPath([NotNull] string path)
        {
            var lastIndex = 0;
            var indexOfSlash = path.IndexOf('/');
            while (indexOfSlash != -1)
            {
                yield return path.Substring(lastIndex, indexOfSlash - lastIndex + 1);
                lastIndex = indexOfSlash + 1;
                indexOfSlash = path.IndexOf('/', lastIndex);
            }

            var remaining = path.Substring(lastIndex);
            if (!string.IsNullOrEmpty(remaining))
                yield return remaining;
        }

        [NotNull]
        private static IEnumerable<PathElement> ToPathElements([NotNull][ItemNotNull] IEnumerable<string> pathParts)
        {
            foreach (var pathPart in pathParts)
            {
                var isDirectory = pathPart.EndsWith("/");
                var name = isDirectory ? pathPart.Substring(0, pathPart.Length - 1) : pathPart;
                yield return new PathElement(pathPart, Uri.UnescapeDataString(name), isDirectory);
            }
        }

        [NotNull]
        [ItemNotNull]
        private Task<SelectionResult> TraverseAsync([NotNull] ICollection currentCollection, [NotNull][ItemNotNull] IEnumerable<string> pathParts, CancellationToken ct)
        {
            return TraverseAsync(currentCollection, ToPathElements(pathParts), ct);
        }

        [NotNull]
        [ItemNotNull]
        private async Task<SelectionResult> TraverseAsync([NotNull] IFileSystem fileSystem, [NotNull][ItemNotNull] IEnumerable<string> pathParts, CancellationToken ct)
        {
            var current = await fileSystem.Root.ConfigureAwait(false);
            return await TraverseAsync(current, pathParts, ct).ConfigureAwait(false);
        }

        [NotNull]
        [ItemNotNull]
        private async Task<SelectionResult> TraverseAsync([NotNull] ICollection startCollection, [NotNull] IEnumerable<PathElement> pathParts, CancellationToken ct)
        {
            var currentPathStack = new Stack<ICollection>();

            var currentCollection = startCollection;
            currentPathStack.Push(currentCollection);

            var pathPartsArr = pathParts.ToArray();

            var id = Guid.NewGuid().ToString("D");
            for (var i = 0; i != pathPartsArr.Length; ++i)
            {
                var pathPart = pathPartsArr[i];
                if (_logger?.IsEnabled(LogLevel.Trace) ?? false)
                    _logger.LogTrace($"Processing path ({id}), part {pathPart.Name} ({pathPart.OriginalName})");

                if (pathPart.OriginalName == "./" || pathPart.OriginalName == string.Empty)
                    continue;
                if (pathPart.OriginalName == "../")
                {
                    currentCollection = currentPathStack.Pop();
                    continue;
                }

                var next = await currentCollection.GetChildAsync(pathPart.Name, ct).ConfigureAwait(false);
                if (next == null)
                {
                    // missing
                    var missingPathParts = pathPartsArr.Select(x => x.Name).Skip(i).ToArray();
                    if (_logger?.IsEnabled(LogLevel.Debug) ?? false)
                        _logger.LogDebug($"Processing path ({id}), missing {pathPart.Name} ({pathPart.OriginalName}) with ({string.Join("/", missingPathParts)}) following");
                    return SelectionResult.CreateMissingDocumentOrCollection(currentCollection, missingPathParts);
                }

                var isDirectory = next is ICollection;
                if (pathPart.IsDirectory && !isDirectory)
                {
                    // file instead of directory
                    var missingPathParts = pathPartsArr.Select(x => x.Name).Skip(i).ToArray();
                    if (_logger?.IsEnabled(LogLevel.Debug) ?? false)
                        _logger.LogDebug($"Processing path ({id}), missing collection {pathPart.Name} ({pathPart.OriginalName}) with ({string.Join("/", missingPathParts)}) following");
                    return SelectionResult.CreateMissingCollection(currentCollection, missingPathParts);
                }

                if (!isDirectory)
                {
                    // file found
                    if (_logger?.IsEnabled(LogLevel.Debug) ?? false)
                        _logger.LogDebug($"Processing path ({id}), found document {next.Name} ({next.Path})");
                    return SelectionResult.Create(currentCollection, (IDocument)next);
                }

                currentCollection = (ICollection)next;
                currentPathStack.Push(currentCollection);
            }

            // directory found
            if (_logger?.IsEnabled(LogLevel.Debug) ?? false)
                _logger.LogDebug($"Processing path ({id}), found collection {currentCollection.Name} ({currentCollection.Path})");
            return SelectionResult.Create(currentCollection);
        }

        private struct PathElement
        {
            public PathElement(string originalName, string name, bool isDirectory)
            {
                OriginalName = originalName;
                Name = name;
                IsDirectory = isDirectory;
            }

            public string OriginalName { get; }

            public string Name { get; }

            public bool IsDirectory { get; }
        }
    }
}