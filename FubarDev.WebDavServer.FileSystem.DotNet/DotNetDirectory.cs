using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties;

using JetBrains.Annotations;

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

            return Task.FromResult<IEntry>(CreateEntry(item));
        }

        public Task<IReadOnlyCollection<IEntry>> GetChildrenAsync(CancellationToken ct)
        {
            var result = new List<IEntry>();
            result.AddRange(GetChildEntries(ct));
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

        public IAsyncEnumerable<IEntry> GetEntries(int maxDepth)
        {
            return this.EnumerateEntries(maxDepth);
        }

        public Task<CollectionActionResult> CopyToAsync(ICollection collection, bool recursive, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<CollectionActionResult> CopyToAsync(ICollection collection, string name, bool recursive, CancellationToken cancellationToken)
        {
            var targetDir = (DotNetDirectory)collection;
            var engine = new RecursiveItemEngine<DotNetEntry, DotNetDirectory, DotNetFile, DotNetItemInfo>(this, targetDir, new CopyActions());
            var remainingDepth = recursive ? int.MaxValue : 0;
            await engine.StartAsync(name, remainingDepth, cancellationToken).ConfigureAwait(false);
            if (engine.ActionInfo.ErrorStatusCode != WebDavStatusCodes.OK)
            {
                
            }
            /*
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
            */
            throw new NotImplementedException();
        }

        public Task<CollectionActionResult> MoveToAsync(ICollection collection, bool recursive, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<CollectionActionResult> MoveToAsync(ICollection collection, string name, bool recursive, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private DotNetEntry CreateEntry(FileSystemInfo fsInfo)
        {
            var fileInfo = fsInfo as FileInfo;
            if (fileInfo != null)
                return new DotNetFile(DotNetFileSystem, fileInfo, Path.Append(Uri.EscapeDataString(fileInfo.Name)));

            var dirInfo = (DirectoryInfo) fsInfo;
            return new DotNetDirectory(DotNetFileSystem, dirInfo, Path.Append(Uri.EscapeDataString(dirInfo.Name) + "/"));
        }

        public IReadOnlyCollection<DotNetEntry> GetChildEntries(CancellationToken ct)
        {
            var result = new List<DotNetEntry>();
            foreach (var info in DirectoryInfo.EnumerateFileSystemInfos())
            {
                ct.ThrowIfCancellationRequested();
                var entry = CreateEntry(info);
                var ignoreEntry = _fileSystemPropertyStore?.IgnoreEntry(entry) ?? false;
                if (!ignoreEntry)
                    result.Add(entry);
            }

            return result;
        }

        private class DotNetItemInfo : IInfo
        {
            public DotNetItemInfo([NotNull] DotNetEntry item, [NotNull, ItemNotNull] IReadOnlyCollection<XElement> properties)
            {
                Name = item.Name;
                CreationDateTime = item.Info.CreationTimeUtc;
                ModificationDateTime = item.Info.LastWriteTimeUtc;
                Properties = properties;
            }

            public DateTime CreationDateTime { get; }
            public DateTime ModificationDateTime { get; }
            public string Name { get; }
            public IReadOnlyCollection<XElement> Properties { get; }
        }

        private interface IInfo
        {
            [NotNull]
            string Name { get; }

            [NotNull, ItemNotNull]
            IReadOnlyCollection<XElement> Properties { get; }
        }

        private class CopyActions : IElementActions<DotNetEntry, DotNetDirectory, DotNetFile, DotNetItemInfo>
        {
            public Task<IReadOnlyCollection<DotNetEntry>> GetChildrenAsync(DotNetDirectory directory, CancellationToken ct)
            {
                return Task.FromResult(directory.GetChildEntries(ct));
            }

            public async Task<DotNetItemInfo> GetInfoAsync(DotNetEntry item, CancellationToken ct)
            {
                var properties = new List<XElement>();
                using (var enumerator = item.GetProperties().GetEnumerator())
                {
                    while (await enumerator.MoveNext(ct).ConfigureAwait(false))
                    {
                        properties.Add(await enumerator.Current.GetXmlValueAsync(ct).ConfigureAwait(false));
                    }
                }

                return new DotNetItemInfo(item, properties);
            }

            public Task SetInfoAsync(DotNetEntry item, DotNetItemInfo info, CancellationToken ct)
            {
                item.Info.CreationTimeUtc = info.CreationDateTime;
                item.Info.LastWriteTimeUtc = info.ModificationDateTime;
                return Task.FromResult(0);
            }

            public Task<DotNetFile> ExecuteActionAsync(DotNetFile item, DotNetDirectory targetDirectory, string targetName, CancellationToken ct)
            {
                var targetPath = System.IO.Path.Combine(targetDirectory.DirectoryInfo.FullName, targetName);
                item.FileInfo.CopyTo(targetPath, true);
                return Task.FromResult((DotNetFile) targetDirectory.CreateEntry(new FileInfo(targetPath)));
            }

            public Task<DotNetDirectory> ExecuteActionAsync(DotNetDirectory item, DotNetDirectory targetDirectory, string targetName, CancellationToken ct)
            {
                var targetPath = System.IO.Path.Combine(targetDirectory.DirectoryInfo.FullName, targetName);
                return Task.FromResult((DotNetDirectory) targetDirectory.CreateEntry(targetDirectory.DirectoryInfo.CreateSubdirectory(targetPath)));
            }

            public Task ExecuteActionAsync(DotNetDirectory item, DotNetDirectory targetDirectory, CancellationToken ct)
            {
                targetDirectory.DirectoryInfo.Create();
                return Task.FromResult(0);
            }
        }

        private interface IElementActions<TItem, TDirectory, TFile, TItemInfo> 
            where TItem : class
            where TDirectory : class, TItem
            where TFile : class, TItem
            where TItemInfo : IInfo
        {
            [NotNull, ItemNotNull]
            Task<IReadOnlyCollection<TItem>> GetChildrenAsync([NotNull] TDirectory directory, CancellationToken ct);

            [NotNull, ItemNotNull]
            Task<TItemInfo> GetInfoAsync([NotNull] TItem item, CancellationToken ct);

            [NotNull]
            Task SetInfoAsync([NotNull] TItem item, [NotNull] TItemInfo info, CancellationToken ct);

            [NotNull, ItemNotNull]
            Task<TFile> ExecuteActionAsync([NotNull] TFile item, [NotNull] TDirectory targetDirectory, [NotNull] string targetName, CancellationToken ct);

            [NotNull, ItemNotNull]
            Task<TDirectory> ExecuteActionAsync([NotNull] TDirectory item, [NotNull] TDirectory targetDirectory, [NotNull] string targetName, CancellationToken ct);

            [NotNull]
            Task ExecuteActionAsync([NotNull] TDirectory item, [NotNull] TDirectory targetDirectory, CancellationToken ct);
        }

        private class RecursiveItemEngine<TItem, TDirectory, TFile, TItemInfo>
            where TItem : class
            where TDirectory : class, TItem
            where TFile : class, TItem
            where TItemInfo : IInfo
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

            public Task StartAsync(int remainingDepth, CancellationToken cancellationToken)
            {
                return ExecuteAsync(_sourceDirectory, _targetDirectory, remainingDepth, cancellationToken);
            }

            public async Task StartAsync(string name, int remainingDepth, CancellationToken cancellationToken)
            {
                var targetDir = await _itemHandler.ExecuteActionAsync(_sourceDirectory, _targetDirectory, name, cancellationToken).ConfigureAwait(false);
                if (remainingDepth == 0)
                    return;
                await ExecuteAsync(_sourceDirectory, targetDir, remainingDepth == int.MaxValue ? remainingDepth : remainingDepth - 1, cancellationToken).ConfigureAwait(false);
            }

            private async Task<bool> ExecuteAsync(TDirectory sourceDirectory, TDirectory targetDirectory, int remainingDepth, CancellationToken cancellationToken)
            {
                var sourceInfo = await _itemHandler.GetInfoAsync(sourceDirectory, cancellationToken).ConfigureAwait(false);
                await _itemHandler.ExecuteActionAsync(sourceDirectory, targetDirectory, cancellationToken).ConfigureAwait(false);
                ActionInfo.Directories.Add(Tuple.Create(sourceDirectory, targetDirectory));
                if (remainingDepth != 0)
                {
                    var children = await _itemHandler.GetChildrenAsync(sourceDirectory, cancellationToken).ConfigureAwait(false);
                    var result = await ExecuteAsync(children, targetDirectory, remainingDepth, cancellationToken).ConfigureAwait(false);
                    if (!result)
                        return false;
                }

                await _itemHandler.SetInfoAsync(targetDirectory, sourceInfo, cancellationToken).ConfigureAwait(false);
                return true;
            }

            private async Task<bool> ExecuteAsync(IEnumerable<TItem> children, TDirectory targetDirectory, int remainingDepth, CancellationToken cancellationToken)
            {
                foreach (var child in children)
                {
                    var sourceInfo = await _itemHandler.GetInfoAsync(child, cancellationToken).ConfigureAwait(false);
                    var fileInfo = child as TFile;
                    if (fileInfo != null)
                    {
                        try
                        {
                            var targetFile = await _itemHandler
                                .ExecuteActionAsync(fileInfo, targetDirectory, sourceInfo.Name, cancellationToken)
                                .ConfigureAwait(false);
                            ActionInfo.Files.Add(Tuple.Create(fileInfo, targetFile));
                        }
                        catch
                        {
                            ActionInfo.FailedItem = fileInfo;
                            ActionInfo.ErrorStatusCode = WebDavStatusCodes.Conflict;
                            return false;
                        }
                    }
                    else
                    {
                        var dirInfo = (TDirectory) child;
                        try
                        {
                            var targetSubDir = await _itemHandler
                                .ExecuteActionAsync(dirInfo, targetDirectory, sourceInfo.Name, cancellationToken)
                                .ConfigureAwait(false);
                            ActionInfo.Directories.Add(Tuple.Create(dirInfo, targetSubDir));

                            if (remainingDepth != 0)
                            {
                                var subChildren = await _itemHandler.GetChildrenAsync(dirInfo, cancellationToken).ConfigureAwait(false);
                                var newRemainingDepth = remainingDepth == int.MaxValue ? remainingDepth : remainingDepth - 1;
                                await ExecuteAsync(subChildren, targetSubDir, newRemainingDepth, cancellationToken).ConfigureAwait(false);
                            }
                        }
                        catch
                        {
                            ActionInfo.FailedItem = dirInfo;
                            ActionInfo.ErrorStatusCode = WebDavStatusCodes.Conflict;
                            return false;
                        }
                    }

                    await _itemHandler.SetInfoAsync(child, sourceInfo, cancellationToken).ConfigureAwait(false);
                }

                return true;
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
