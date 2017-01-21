namespace FubarDev.WebDavServer.FileSystem
{
    public interface ILocalFileSystem : IFileSystem
    {
        string RootDirectoryPath { get; }
    }
}
