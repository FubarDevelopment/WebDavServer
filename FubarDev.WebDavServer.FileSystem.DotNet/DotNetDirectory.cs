using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    public class DotNetDirectory : DotNetEntry, ICollection, IRecusiveChildrenCollector
    {
        private readonly IFileSystemPropertyStore _fileSystemPropertyStore;

        public DotNetDirectory(DotNetFileSystem fileSystem, DirectoryInfo info, Uri path)
            : base(fileSystem, info, path)
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
            info.Create();
            return Task.FromResult((ICollection)CreateEntry(info));
        }

        public override Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken)
        {
            DirectoryInfo.Delete(true);
            return Task.FromResult(new DeleteResult(WebDavStatusCodes.OK, null));
        }

        public Task<CollectionActionResult> CopyToAsync(ICollection collection, string name, CancellationToken cancellationToken)
        {
            var dir = (DotNetDirectory)collection;
            var targetDirectoryName = System.IO.Path.Combine(dir.DirectoryInfo.FullName, name);
            var targetDirInfo = Directory.CreateDirectory(targetDirectoryName);
            var engine = new ExecuteRecursiveAction(
                DirectoryInfo,
                targetDirInfo,
                (src, dst) =>
                {
                    if (dst.Exists)
                        dst.Delete();
                    src.MoveTo(dst.FullName);
                },
                (src, dst) =>
                {
                    dst.Create();
                });
            throw new NotImplementedException();
        }

        public Task<CollectionActionResult> MoveToAsync(ICollection collection, string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private IEntry CreateEntry(FileSystemInfo fsInfo)
        {
            var fileInfo = fsInfo as FileInfo;
            if (fileInfo != null)
                return new DotNetFile(DotNetFileSystem, fileInfo, Path.Append(Uri.EscapeDataString(fileInfo.Name)));

            var dirInfo = (DirectoryInfo) fsInfo;
            return new DotNetDirectory(DotNetFileSystem, dirInfo, Path.Append(Uri.EscapeDataString(dirInfo.Name) + "/"));
        }

        private class ExecuteRecursiveAction
        {
            private readonly DirectoryInfo _sourceDirectory;
            private readonly DirectoryInfo _targetDirectory;
            private readonly Action<FileInfo, FileInfo> _fileAction;
            private readonly Action<DirectoryInfo, DirectoryInfo> _directoryAction;

            public ExecuteRecursiveAction(
                DirectoryInfo sourceDirectory,
                DirectoryInfo targetDirectory,
                Action<FileInfo, FileInfo> fileAction,
                Action<DirectoryInfo, DirectoryInfo> directoryAction)
            {
                _sourceDirectory = sourceDirectory;
                _targetDirectory = targetDirectory;
                _fileAction = fileAction;
                _directoryAction = directoryAction;
            }

            public ActionInfo ActionInfo { get; } = new ActionInfo();

            public Task StartAsync(CancellationToken cancellationToken)
            {
                return ExecuteAsync(_sourceDirectory, _targetDirectory, cancellationToken);
            }

            private Task ExecuteAsync(DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        public IAsyncEnumerable<IEntry> GetEntries(int maxDepth)
        {
            return this.EnumerateEntries(maxDepth);
        }

        private class ActionInfo
        {
            public List<Tuple<DirectoryInfo, DirectoryInfo>> Directories { get; } = new List<Tuple<DirectoryInfo, DirectoryInfo>>();
            public List<Tuple<FileInfo, FileInfo>> Files { get; } = new List<Tuple<FileInfo, FileInfo>>();
            public FileSystemInfo FailedItem { get; set; }
            public WebDavStatusCodes ErrorStatusCode { get; set; } = WebDavStatusCodes.OK;
        }
    }
}
