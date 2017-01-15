using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Properties
{
    public interface IFileSystemPropertyStore : IPropertyStore
    {
        bool IgnoreEntry(IEntry entry);
        string RootPath { get; set; }
    }
}
