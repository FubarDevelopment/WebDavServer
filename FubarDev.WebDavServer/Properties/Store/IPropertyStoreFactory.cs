using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Properties.Store
{
    public interface IPropertyStoreFactory
    {
        IPropertyStore Create(IFileSystem fileSystem);
    }
}
