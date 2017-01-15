namespace FubarDev.WebDavServer.Properties.Store
{
    public interface IFileSystemPropertyStore : IPropertyStore
    {
        string RootPath { get; set; }
    }
}
