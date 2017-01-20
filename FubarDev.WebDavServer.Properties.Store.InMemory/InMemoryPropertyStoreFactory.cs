using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Properties.Store.InMemory
{
    public class InMemoryPropertyStoreFactory : IPropertyStoreFactory
    {
        public IPropertyStore Create(IFileSystem fileSystem)
        {
            return new InMemoryPropertyStore();
        }
    }
}
