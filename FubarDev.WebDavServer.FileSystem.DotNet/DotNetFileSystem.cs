using System;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    public class DotNetFileSystem : IFileSystem
    {
        private readonly DotNetDirectory _root;

        public DotNetFileSystem(DotNetDirectory rootDirectory)
        {
            _root = rootDirectory;
        }

        public ICollection Root => _root;

        public Task<SelectionResult> SelectAsync(string path, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
