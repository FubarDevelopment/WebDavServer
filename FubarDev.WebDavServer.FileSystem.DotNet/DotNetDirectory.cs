using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties.Store;

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
            return Task.FromResult(new DeleteResult(WebDavStatusCodes.OK, null));
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

            var dirInfo = (DirectoryInfo) fsInfo;
            return new DotNetDirectory(DotNetFileSystem, this, dirInfo, Path.AppendDirectory(dirInfo.Name));
        }

        private class DotNetItemInfo
        {
            public DotNetItemInfo(FileSystemInfo item)
            {
                CreationDateTime = item.CreationTimeUtc;
                ModificationDateTime = item.LastWriteTimeUtc;
            }

            public DateTime CreationDateTime { get; }
            public DateTime ModificationDateTime { get; }
        }

        private class CopyActions : IElementActions<FileSystemInfo, DirectoryInfo, FileInfo, DotNetItemInfo>
        {
            public Task<IEnumerable<FileSystemInfo>> GetChildrenAsync(DirectoryInfo directory, CancellationToken ct)
            {
                return Task.FromResult(directory.EnumerateFileSystemInfos());
            }

            public Task<DotNetItemInfo> GetInfoAsync(FileSystemInfo item, CancellationToken ct)
            {
                return Task.FromResult(new DotNetItemInfo(item));
            }

            public Task SetInfoAsync(FileSystemInfo item, DotNetItemInfo info, CancellationToken ct)
            {
                item.CreationTimeUtc = info.CreationDateTime;
                item.LastWriteTimeUtc = info.ModificationDateTime;
                return Task.FromResult(0);
            }

            public Task<FileInfo> ExecuteActionAsync(FileInfo item, DirectoryInfo targetDirectory, string targetName, CancellationToken ct)
            {
                var targetPath = System.IO.Path.Combine(targetDirectory.FullName, targetName);
                item.CopyTo(targetPath, true);
                return Task.FromResult(new FileInfo(targetPath));
            }

            public Task<DirectoryInfo> ExecuteActionAsync(DirectoryInfo item, DirectoryInfo targetDirectory, string targetName, CancellationToken ct)
            {
                var targetPath = System.IO.Path.Combine(targetDirectory.FullName, targetName);
                return Task.FromResult(targetDirectory.CreateSubdirectory(targetPath));
            }

            public Task ExecuteActionAsync(DirectoryInfo item, DirectoryInfo targetDirectory, CancellationToken ct)
            {
                targetDirectory.Create();
                return Task.FromResult(0);
            }
        }

        private interface IElementActions<TItem, TDirectory, TFile, TItemInfo> 
            where TItem : class 
            where TDirectory : class, TItem
            where TFile : class, TItem
        {
            Task<IEnumerable<TItem>> GetChildrenAsync(TDirectory directory, CancellationToken ct);
            Task<TItemInfo> GetInfoAsync(TItem item, CancellationToken ct);
            Task SetInfoAsync(TItem item, DotNetItemInfo info, CancellationToken ct);
            Task<TFile> ExecuteActionAsync(TFile item, TDirectory targetDirectory, string targetName, CancellationToken ct);
            Task<TDirectory> ExecuteActionAsync(TDirectory item, TDirectory targetDirectory, string targetName, CancellationToken ct);
            Task ExecuteActionAsync(TDirectory item, TDirectory targetDirectory, CancellationToken ct);
        }

        private class RecursiveItemEngine<TItem, TDirectory, TFile, TItemInfo>
            where TItem : class
            where TDirectory : class, TItem
            where TFile : class, TItem
        {
            private readonly TDirectory _sourceDirectory;
            private readonly TDirectory _targetDirectory;
            private readonly IElementActions<TItem, TDirectory, TFile, TItemInfo> _itemHandler;

            public RecursiveItemEngine(
                TDirectory sourceDirectory,
                TDirectory targetDirectory,
                IElementActions<TItem, TDirectory, TFile, TItemInfo> itemHandler)
            {
                _sourceDirectory = sourceDirectory;
                _targetDirectory = targetDirectory;
                _itemHandler = itemHandler;
            }

            public ActionInfo<TItem, TDirectory, TFile> ActionInfo { get; } = new ActionInfo<TItem, TDirectory, TFile>();

            public Task StartAsync(CancellationToken cancellationToken)
            {
                return ExecuteAsync(_sourceDirectory, _targetDirectory, cancellationToken);
            }

            private Task ExecuteAsync(TDirectory sourceDirectory, TDirectory targetDirectory, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private class ActionInfo<TItem, TDirectory, TFile>
            where TItem : class
            where TDirectory : class, TItem
            where TFile : class, TItem
        {
            public List<Tuple<TDirectory, TDirectory>> Directories { get; } = new List<Tuple<TDirectory, TDirectory>>();
            public List<Tuple<TFile, TFile>> Files { get; } = new List<Tuple<TFile, TFile>>();
            public TItem FailedItem { get; set; }
            public WebDavStatusCodes ErrorStatusCode { get; set; } = WebDavStatusCodes.OK;
        }
    }
}
