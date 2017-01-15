using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Properties;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    public class DotNetDirectory : DotNetEntry, ICollection
    {
        private readonly IFileSystemPropertyStore _fileSystemPropertyStore;

        public DotNetDirectory(DotNetFileSystem fileSystem, DirectoryInfo info, string path)
            : base(fileSystem, info, path)
        {
            _fileSystemPropertyStore = fileSystem.PropertyStore as IFileSystemPropertyStore;
            DirectoryInfo = info;
        }

        public DirectoryInfo DirectoryInfo { get; }

        public Task<IEntry> GetChildAsync(string name, CancellationToken ct)
        {
            var items = DirectoryInfo.GetFileSystemInfos(name);
            if (items.Length != 1)
                return Task.FromResult<IEntry>(null);
            return Task.FromResult(CreateEntry(items[0]));
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

        private IEntry CreateEntry(FileSystemInfo fsInfo)
        {
            var fileInfo = fsInfo as FileInfo;
            if (fileInfo != null)
                return new DotNetFile(FileSystem, fileInfo, Path + fileInfo.Name);

            var dirInfo = (DirectoryInfo) fsInfo;
            return new DotNetDirectory(FileSystem, dirInfo, Path + dirInfo.Name + "/");
        }
    }
}
