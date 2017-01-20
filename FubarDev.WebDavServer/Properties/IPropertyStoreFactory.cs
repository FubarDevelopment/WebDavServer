using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Properties
{
    public interface IPropertyStoreFactory
    {
        IPropertyStore Create(IFileSystem fileSystem);
    }
}
