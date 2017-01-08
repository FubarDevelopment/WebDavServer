using System.IO;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    public class DotNetFile : DotNetEntry, IDocument
    {
        public DotNetFile(DotNetFileSystem fileSystem, FileInfo info, string path)
            : base(fileSystem, info, path)
        {
            FileInfo = info;
        }

        public FileInfo FileInfo { get; }
    }
}
