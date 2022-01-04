// <copyright file="CollectionExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem.Mount;

namespace FubarDev.WebDavServer.FileSystem
{
    /// <summary>
    /// Extension methods for the collections.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Returns the target if the collection is a mount point or the collection itself.
        /// </summary>
        /// <param name="collection">The collection to found the mount destination for.</param>
        /// <param name="mountPointProvider">The mount point provider.</param>
        /// <returns>The <paramref name="collection"/> or the destination collection if a mount point existed.</returns>
        public static async Task<ICollection> GetMountTargetAsync(this ICollection collection, IMountPointProvider? mountPointProvider)
        {
            if (mountPointProvider != null && mountPointProvider.TryGetMountPoint(collection.Path, out var fileSystem))
            {
                return await fileSystem.Root;
            }

            return collection;
        }

        /// <summary>
        /// Returns the target if the collection is a mount point or the collection itself.
        /// </summary>
        /// <param name="collection">The collection to found the mount destination for.</param>
        /// <param name="mountPointProvider">The mount point provider.</param>
        /// <returns>The <paramref name="collection"/> or the destination collection if a mount point existed.</returns>
        public static async Task<IEntry> GetMountTargetEntryAsync(this ICollection collection, IMountPointProvider? mountPointProvider)
        {
            if (mountPointProvider != null && mountPointProvider.TryGetMountPoint(collection.Path, out var fileSystem))
            {
                return await fileSystem.Root;
            }

            return collection;
        }

        /// <summary>
        /// Gets all entries of a collection recursively.
        /// </summary>
        /// <param name="collection">The collection to get the entries from.</param>
        /// <param name="children">Child items for the given <paramref name="collection"/>.</param>
        /// <param name="maxDepth">The maximum depth (0 = only entries of the <paramref name="collection"/>, but not of its sub collections).</param>
        /// <returns>An async enumerable to collect all the entries recursively.</returns>
        public static IAsyncEnumerable<IEntry> EnumerateEntries(this ICollection collection, IReadOnlyCollection<IEntry> children, int maxDepth)
        {
            return new FileSystemEntries(collection, children, 0, maxDepth);
        }

        /// <summary>
        /// Gets all entries of a collection recursively.
        /// </summary>
        /// <param name="collection">The collection to get the entries from.</param>
        /// <param name="maxDepth">The maximum depth (0 = only entries of the <paramref name="collection"/>, but not of its sub collections).</param>
        /// <returns>An async enumerable to collect all the entries recursively.</returns>
        public static IAsyncEnumerable<IEntry> EnumerateEntries(this ICollection collection, int maxDepth)
        {
            return new FileSystemEntries(collection, null, 0, maxDepth);
        }

        /// <summary>
        /// Gets the collection as node.
        /// </summary>
        /// <param name="collection">The collection to get the node for.</param>
        /// <param name="maxDepth">The maximum depth to be used to get the child nodes.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The collection node.</returns>
        public static async Task<ICollectionNode> GetNodeAsync(this ICollection collection, int maxDepth, CancellationToken cancellationToken)
        {
            var result = new NodeInfo(collection);
            if (maxDepth <= 0)
            {
                return result;
            }

            var subNodeQueue = new Queue<NodeInfo>();
            var current = result;

            var entries = EnumerateEntries(collection, maxDepth - 1);
            await foreach (var entry in entries.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var parent = entry.Parent;
                while (parent != current.Collection)
                {
                    current = subNodeQueue.Dequeue();
                }

                var doc = entry as IDocument;
                if (doc == null)
                {
                    var coll = (ICollection)entry;
                    var info = new NodeInfo(coll);
                    current.SubNodes.Add(info);
                    subNodeQueue.Enqueue(info);
                }
                else
                {
                    current.Documents.Add(doc);
                }
            }

            return result;
        }

        private class FileSystemEntries : IAsyncEnumerable<IEntry>
        {
            private readonly ICollection _collection;

            private readonly IReadOnlyCollection<IEntry>? _children;

            private readonly int _remainingDepth;

            private readonly int _startDepth;

            public FileSystemEntries(ICollection collection, IReadOnlyCollection<IEntry>? children, int startDepth, int remainingDepth)
            {
                _collection = collection;
                _children = children;
                _startDepth = startDepth;
                _remainingDepth = remainingDepth;
            }

            public IAsyncEnumerator<IEntry> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new FileSystemEntriesEnumerator(_collection, _children, _startDepth, _remainingDepth, cancellationToken);
            }

            private class FileSystemEntriesEnumerator : IAsyncEnumerator<IEntry>
            {
                private readonly Queue<CollectionInfo> _collections = new Queue<CollectionInfo>();
                private readonly int _maxDepth;
                private readonly CancellationToken _cancellationToken;
                private ICollection? _collection;
                private int _currentDepth;
                private IEnumerator<IEntry>? _entries;
                private IEntry? _current;

                public FileSystemEntriesEnumerator(
                    ICollection collection,
                    IReadOnlyCollection<IEntry>? children,
                    int startDepth,
                    int maxDepth,
                    CancellationToken cancellationToken)
                {
                    _maxDepth = maxDepth;
                    _cancellationToken = cancellationToken;
                    _currentDepth = startDepth;
                    _collections.Enqueue(new CollectionInfo(collection, children, startDepth));
                }

                [AllowNull]
                public IEntry Current => _current ?? throw new InvalidOperationException("No item available.");

                public ValueTask DisposeAsync()
                {
                    _entries?.Dispose();
                    return default;
                }

                public async ValueTask<bool> MoveNextAsync()
                {
                    var resultFound = false;
                    var hasCurrent = false;

                    while (!resultFound)
                    {
                        _cancellationToken.ThrowIfCancellationRequested();

                        if (_entries == null)
                        {
                            var nextCollectionInfo = _collections.Dequeue();
                            _collection = nextCollectionInfo.Collection;
                            _currentDepth = nextCollectionInfo.Depth;
                            var children = nextCollectionInfo.Children ?? await _collection.GetChildrenAsync(_cancellationToken).ConfigureAwait(false);
                            _entries = children.GetEnumerator();
                        }

                        if (_entries.MoveNext())
                        {
                            var coll = _entries.Current as ICollection;
                            if (_currentDepth < _maxDepth && coll != null)
                            {
                                IReadOnlyCollection<IEntry> children;
                                try
                                {
                                    children = await coll.GetChildrenAsync(_cancellationToken).ConfigureAwait(false);
                                }
                                catch (Exception)
                                {
                                    // Ignore errors
                                    children = new IEntry[0];
                                }

                                var collectionInfo = new CollectionInfo(coll, children, _currentDepth + 1);
                                _collections.Enqueue(collectionInfo);
                            }

                            if (_currentDepth >= 0)
                            {
                                _current = _entries.Current;
                                resultFound = true;
                                hasCurrent = true;
                            }
                        }
                        else
                        {
                            _current = null;
                            _entries.Dispose();
                            _entries = null;
                            resultFound = _collections.Count == 0;
                        }
                    }

                    return hasCurrent;
                }

                private struct CollectionInfo
                {
                    public CollectionInfo(ICollection collection, IReadOnlyCollection<IEntry>? children, int depth)
                    {
                        Collection = collection;
                        Children = children;
                        Depth = depth;
                    }

                    public ICollection Collection { get; }

                    public IReadOnlyCollection<IEntry>? Children { get; }

                    public int Depth { get; }
                }
            }
        }

        private class NodeInfo : ICollectionNode
        {
            public NodeInfo(ICollection collection)
            {
                Collection = collection;
            }

            public string Name => Collection.Name;

            public ICollection Collection { get; }

            public List<IDocument> Documents { get; } = new List<IDocument>();

            public List<NodeInfo> SubNodes { get; } = new List<NodeInfo>();

            IReadOnlyCollection<ICollectionNode> ICollectionNode.Nodes => SubNodes;

            IReadOnlyCollection<IDocument> ICollectionNode.Documents => Documents;
        }
    }
}
