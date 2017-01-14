using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    public class DotNetDirectory : DotNetEntry, ICollection
    {
        public DotNetDirectory(DotNetFileSystem fileSystem, DirectoryInfo info, string path)
            : base(fileSystem, info, path)
        {
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
                result.Add(CreateEntry(info));
            }

            return Task.FromResult<IReadOnlyCollection<IEntry>>(result);
        }

        private IEntry CreateEntry(FileSystemInfo fsInfo)
        {
            var fileInfo = fsInfo as FileInfo;
            if (fileInfo != null)
                return new DotNetFile(FileSystem, fileInfo, this.Path + fileInfo.Name);

            var dirInfo = (DirectoryInfo) fsInfo;
            return new DotNetDirectory(FileSystem, dirInfo, this.Path + dirInfo.Name + "/");
        }
    }
}
