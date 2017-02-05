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

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    public class DotNetDirectory : DotNetEntry, ICollection, IRecusiveChildrenCollector
    {
        private readonly IFileSystemPropertyStore _fileSystemPropertyStore;

        public DotNetDirectory(DotNetFileSystem fileSystem, DotNetDirectory parent, DirectoryInfo info, Uri path)
            : base(fileSystem, parent, info, path)
        {
            _fileSystemPropertyStore = fileSystem.PropertyStore as IFileSystemPropertyStore;
            DirectoryInfo = info;
        }

        public DirectoryInfo DirectoryInfo { get; }

        public Task<IEntry> GetChildAsync(string name, CancellationToken ct)
        {
            var newPath = System.IO.Path.Combine(DirectoryInfo.FullName, name);

            FileSystemInfo item = new FileInfo(newPath);
            if (!item.Exists)
                item = new DirectoryInfo(newPath);

            if (!item.Exists)
                return Task.FromResult<IEntry>(null);

            return Task.FromResult(CreateEntry(item));
        }

        public Task<IReadOnlyCollection<IEntry>> GetChildrenAsync(CancellationToken ct)
        {
            var result = new List<IEntry>();
            foreach (var info in DirectoryInfo.EnumerateFileSystemInfos())
            {
                ct.ThrowIfCancellationRequested();
                var entry = CreateEntry(info);
                var ignoreEntry = _fileSystemPropertyStore?.IgnoreEntry(entry) ?? false;
                if (!ignoreEntry)
                    result.Add(entry);
            }

            return Task.FromResult<IReadOnlyCollection<IEntry>>(result);
        }

        public Task<IDocument> CreateDocumentAsync(string name, CancellationToken cancellationToken)
        {
            var info = new FileInfo(System.IO.Path.Combine(DirectoryInfo.FullName, name));
            info.Create().Dispose();
            return Task.FromResult((IDocument)CreateEntry(info));
        }

        public Task<ICollection> CreateCollectionAsync(string name, CancellationToken cancellationToken)
        {
            var info = new DirectoryInfo(System.IO.Path.Combine(DirectoryInfo.FullName, name));
            if (info.Exists)
                throw new IOException("Collection already exists.");
            info.Create();
            return Task.FromResult((ICollection)CreateEntry(info));
        }

        public override Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken)
        {
            DirectoryInfo.Delete(true);
            return Task.FromResult(new DeleteResult(WebDavStatusCode.OK, null));
        }

        public IAsyncEnumerable<IEntry> GetEntries(int maxDepth)
        {
            return this.EnumerateEntries(maxDepth);
        }

        private IEntry CreateEntry(FileSystemInfo fsInfo)
        {
            var fileInfo = fsInfo as FileInfo;
            if (fileInfo != null)
                return new DotNetFile(DotNetFileSystem, this, fileInfo, Path.Append(fileInfo.Name, false));

            var dirInfo = (DirectoryInfo)fsInfo;
            return new DotNetDirectory(DotNetFileSystem, this, dirInfo, Path.AppendDirectory(dirInfo.Name));
        }
    }
}
