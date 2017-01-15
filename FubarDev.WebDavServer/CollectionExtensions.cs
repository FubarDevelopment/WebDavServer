using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Gets all entries of a collection recursively
        /// </summary>
        /// <param name="collection">The collection to get the entries from</param>
        /// <param name="children">Child items for the given <paramref name="collection"/></param>
        /// <param name="maxDepth">The maximum depth (0 = only entries of the <paramref name="collection"/>, but not of its sub collections)</param>
        /// <returns>An async enumerable to collect all the entries recursively</returns>
        public static IAsyncEnumerable<IEntry> GetEntries(this ICollection collection, IReadOnlyCollection<IEntry> children, int maxDepth)
        {
            return new FileSystemEntries(collection, children, maxDepth);
        }

        private class FileSystemEntries : IAsyncEnumerable<IEntry>
        {
            private readonly ICollection _collection;

            private readonly IReadOnlyCollection<IEntry> _children;

            private readonly int _remainingDepth;

            public FileSystemEntries(ICollection collection, IReadOnlyCollection<IEntry> children, int remainingDepth)
            {
                _collection = collection;
                _children = children;
                _remainingDepth = remainingDepth;
            }

            public IAsyncEnumerator<IEntry> GetEnumerator()
            {
                return new FileSystemEntriesEnumerator(_collection, _children, _remainingDepth);
            }

            private class FileSystemEntriesEnumerator : IAsyncEnumerator<IEntry>
            {
                private readonly Queue<CollectionInfo> _collections = new Queue<CollectionInfo>();

                private readonly int _maxDepth;

                private ICollection _collection;

                private int _currentDepth;

                private IEnumerator<IEntry> _entries;

                public FileSystemEntriesEnumerator(ICollection collection, IReadOnlyCollection<IEntry> children, int maxDepth)
                {
                    _maxDepth = maxDepth;
                    _currentDepth = 0;
                    _collections.Enqueue(new CollectionInfo(collection, children, 0));
                }

                public IEntry Current { get; private set; }

                public void Dispose()
                {
                    _entries?.Dispose();
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    var resultFound = false;
                    var hasCurrent = false;

                    while (!resultFound)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (_entries == null)
                        {
                            var nextCollectionInfo = _collections.Dequeue();
                            _collection = nextCollectionInfo.Collection;
                            _currentDepth = nextCollectionInfo.Depth;
                            var children = nextCollectionInfo.Children ?? await _collection.GetChildrenAsync(cancellationToken).ConfigureAwait(false);
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
                                    children = await coll.GetChildrenAsync(cancellationToken).ConfigureAwait(false);
                                }
                                catch (Exception)
                                {
                                    // Ignore errors
                                    children = new IEntry[0];
                                }

                                var collectionInfo = new CollectionInfo(coll, children, _currentDepth + 1);
                                _collections.Enqueue(collectionInfo);
                            }

                            Current = _entries.Current;
                            resultFound = true;
                            hasCurrent = true;
                        }
                        else
                        {
                            Current = null;
                            _entries?.Dispose();
                            _entries = null;
                            resultFound = _collections.Count == 0;
                        }
                    }

                    return hasCurrent;
                }

                private struct CollectionInfo
                {
                    public CollectionInfo(ICollection collection, IReadOnlyCollection<IEntry> children, int depth)
                    {
                        Collection = collection;
                        Children = children;
                        Depth = depth;
                    }

                    public ICollection Collection { get; }

                    public IReadOnlyCollection<IEntry> Children { get; }

                    public int Depth { get; }
                }
            }
        }
    }
}
