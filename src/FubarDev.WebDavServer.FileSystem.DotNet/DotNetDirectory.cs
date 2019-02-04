// <copyright file="DotNetDirectory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Props.Store;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    /// <summary>
    /// A .NET <see cref="System.IO"/> based implementation of a WebDAV collection.
    /// </summary>
    public class DotNetDirectory : DotNetEntry, ICollection, IRecursiveChildrenCollector
    {
        private readonly IFileSystemPropertyStore _fileSystemPropertyStore;

        private readonly bool _isRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetDirectory"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system this collection belongs to.</param>
        /// <param name="parent">The parent collection.</param>
        /// <param name="info">The directory information.</param>
        /// <param name="path">The root-relative path of this collection.</param>
        /// <param name="name">The entry name (<see langword="null"/> when <see cref="FileSystemInfo.Name"/> of <see cref="DotNetEntry.Info"/> should be used).</param>
        /// <param name="isRoot">Indicates whether this is the file systems root directory.</param>
        public DotNetDirectory(
            [NotNull] DotNetFileSystem fileSystem,
            [CanBeNull] ICollection parent,
            [NotNull] DirectoryInfo info,
            [NotNull] Uri path,
            [CanBeNull] string name,
            bool isRoot = false)
            : base(fileSystem, parent, info, path, name)
        {
            _isRoot = isRoot;
            _fileSystemPropertyStore = fileSystem.PropertyStore as IFileSystemPropertyStore;
            DirectoryInfo = info;
        }

        /// <summary>
        /// Gets the collections directory information.
        /// </summary>
        public DirectoryInfo DirectoryInfo { get; }

        /// <inheritdoc />
        public Task<IEntry> GetChildAsync(string name, CancellationToken ct)
        {
            var newPath = System.IO.Path.Combine(DirectoryInfo.FullName, name);

            FileSystemInfo item = new FileInfo(newPath);
            if (!item.Exists)
            {
                item = new DirectoryInfo(newPath);
            }

            if (!item.Exists)
            {
                return Task.FromResult<IEntry>(null);
            }

            var entry = CreateEntry(item);

            var coll = entry as ICollection;
            if (coll != null)
            {
                return coll.GetMountTargetEntryAsync(DotNetFileSystem);
            }

            return Task.FromResult(entry);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IEntry>> GetChildrenAsync(CancellationToken ct)
        {
            var result = new List<IEntry>();
            foreach (var info in DirectoryInfo.EnumerateFileSystemInfos())
            {
                ct.ThrowIfCancellationRequested();
                var entry = CreateEntry(info);
                var ignoreEntry = _fileSystemPropertyStore?.IgnoreEntry(entry) ?? false;
                if (!ignoreEntry)
                {
                    var coll = entry as ICollection;
                    if (coll != null)
                    {
                        entry = await coll.GetMountTargetAsync(DotNetFileSystem);
                    }

                    result.Add(entry);
                }
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<IDocument> CreateDocumentAsync(string name, CancellationToken cancellationToken)
        {
            var fullFileName = System.IO.Path.Combine(DirectoryInfo.FullName, name);
            var info = new FileInfo(fullFileName);
            info.Create().Dispose();
            if (FileSystem.PropertyStore != null)
            {
                await FileSystem.PropertyStore.UpdateETagAsync(this, cancellationToken).ConfigureAwait(false);
            }

            return (IDocument)CreateEntry(new FileInfo(fullFileName));
        }

        /// <inheritdoc />
        public async Task<ICollection> CreateCollectionAsync(string name, CancellationToken cancellationToken)
        {
            var fullDirPath = System.IO.Path.Combine(DirectoryInfo.FullName, name);

            var info = new DirectoryInfo(fullDirPath);
            if (info.Exists)
            {
                throw new IOException("Collection already exists.");
            }

            info.Create();

            if (FileSystem.PropertyStore != null)
            {
                await FileSystem.PropertyStore.UpdateETagAsync(this, cancellationToken).ConfigureAwait(false);
            }

            return (ICollection)CreateEntry(new DirectoryInfo(fullDirPath));
        }

        /// <inheritdoc />
        public override async Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken)
        {
            if (_isRoot)
            {
                throw new UnauthorizedAccessException("Cannot remove the file systems root collection");
            }

            var propStore = FileSystem.PropertyStore;
            if (propStore != null)
            {
                await propStore.RemoveAsync(this, cancellationToken).ConfigureAwait(false);
            }

            DirectoryInfo.Delete(true);

            return new DeleteResult(WebDavStatusCode.OK, null);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<IEntry> GetEntries(int maxDepth)
        {
            return this.EnumerateEntries(maxDepth);
        }

        private IEntry CreateEntry(FileSystemInfo fsInfo)
        {
            var fileInfo = fsInfo as FileInfo;
            if (fileInfo != null)
            {
                return new DotNetFile(DotNetFileSystem, this, fileInfo, Path.Append(fileInfo.Name, false));
            }

            var dirInfo = (DirectoryInfo)fsInfo;
            var dirPath = Path.AppendDirectory(dirInfo.Name);
            return new DotNetDirectory(DotNetFileSystem, this, dirInfo, dirPath, null);
        }
    }
}
