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

        public Task<IReadOnlyCollection<IEntry>> GetChildrenAsync(CancellationToken ct)
        {
            var result = new List<IEntry>();
            foreach (var info in DirectoryInfo.EnumerateFileSystemInfos())
            {
                ct.ThrowIfCancellationRequested();

                var fileInfo = info as FileInfo;
                if (fileInfo != null)
                {
                    var path = this.Path + fileInfo.Name;
                    result.Add(new DotNetFile(FileSystem, fileInfo, path));
                }
                else
                {
                    var dirInfo = (DirectoryInfo) info;
                    var path = this.Path + dirInfo.Name + "/";
                    result.Add(new DotNetDirectory(FileSystem, dirInfo, path));
                }
            }

            return Task.FromResult<IReadOnlyCollection<IEntry>>(result);
        }
    }
}
