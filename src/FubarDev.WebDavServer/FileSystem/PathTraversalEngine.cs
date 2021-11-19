// <copyright file="PathTraversalEngine.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.FileSystem
{
    /// <summary>
    /// Helper class to find an entry for a given path.
    /// </summary>
    public class PathTraversalEngine : IPathTraversalEngine
    {
        private readonly ILogger<PathTraversalEngine>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathTraversalEngine"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public PathTraversalEngine(ILogger<PathTraversalEngine>? logger = null)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public Task<SelectionResult> TraverseAsync(IFileSystem fileSystem, string? path, CancellationToken ct)
        {
            return TraverseAsync(fileSystem, SplitPath(path ?? string.Empty), ct);
        }

        /// <inheritdoc />
        public Task<SelectionResult> TraverseAsync(ICollection currentCollection, string? path, CancellationToken ct)
        {
            return TraverseAsync(currentCollection, SplitPath(path ?? string.Empty), ct);
        }

        private static IEnumerable<string> SplitPath(string path)
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
            {
                yield return remaining;
            }
        }

        private static IEnumerable<PathElement> ToPathElements(IEnumerable<string> pathParts)
        {
            foreach (var pathPart in pathParts)
            {
                var isDirectory = pathPart.EndsWith("/");
                var name = isDirectory ? pathPart.Substring(0, pathPart.Length - 1) : pathPart;
                yield return new PathElement(pathPart, name, isDirectory);
            }
        }

        private Task<SelectionResult> TraverseAsync(ICollection currentCollection, IEnumerable<string> pathParts, CancellationToken ct)
        {
            return TraverseAsync(currentCollection, ToPathElements(pathParts), ct);
        }

        private async Task<SelectionResult> TraverseAsync(IFileSystem fileSystem, IEnumerable<string> pathParts, CancellationToken ct)
        {
            var current = await fileSystem.Root.ConfigureAwait(false);
            return await TraverseAsync(current, pathParts, ct).ConfigureAwait(false);
        }

        private async Task<SelectionResult> TraverseAsync(ICollection startCollection, IEnumerable<PathElement> pathParts, CancellationToken ct)
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
                {
                    _logger.LogTrace(
                        "Processing path ({Id}), part {Name} ({OriginalName})",
                        id,
                        pathPart.Name,
                        pathPart.OriginalName);
                }

                if (pathPart.OriginalName == "./" || pathPart.OriginalName == string.Empty)
                {
                    continue;
                }

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
                    {
                        _logger.LogDebug(
                            "Processing path ({Id}), missing {Name} ({OriginalName}) with ({MissingPathFragment}) following",
                            id,
                            pathPart.Name,
                            pathPart.OriginalName,
                            string.Join("/", missingPathParts));
                    }

                    return SelectionResult.CreateMissingDocumentOrCollection(currentCollection, missingPathParts);
                }

                var isDirectory = next is ICollection;
                if (pathPart.IsDirectory && !isDirectory)
                {
                    // file instead of directory
                    var missingPathParts = pathPartsArr.Select(x => x.Name).Skip(i).ToArray();
                    if (_logger?.IsEnabled(LogLevel.Debug) ?? false)
                    {
                        _logger.LogDebug(
                            "Processing path ({Id}), missing collection {Name} ({OriginalName}) with ({MissingPathFragment}) following",
                            id,
                            pathPart.Name,
                            pathPart.OriginalName,
                            string.Join("/", missingPathParts));
                    }

                    return SelectionResult.CreateMissingCollection(currentCollection, missingPathParts);
                }

                if (!isDirectory)
                {
                    // file found
                    if (_logger?.IsEnabled(LogLevel.Debug) ?? false)
                    {
                        _logger.LogDebug(
                            "Processing path ({Id}), found document {Name} ({Path})",
                            id,
                            next.Name,
                            next.Path);
                    }

                    return SelectionResult.Create(currentCollection, (IDocument)next);
                }

                currentCollection = (ICollection)next;
                currentPathStack.Push(currentCollection);
            }

            // directory found
            if (_logger?.IsEnabled(LogLevel.Debug) ?? false)
            {
                _logger.LogDebug(
                    "Processing path ({Id}), found collection {Name} ({Path})",
                    id,
                    currentCollection.Name,
                    currentCollection.Path);
            }

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
