namespace FubarDev.WebDavServer.Properties
{
    public interface IFileSystemPropertyStore : IPropertyStore
    {
        string RootPath { get; set; }
    }
}
