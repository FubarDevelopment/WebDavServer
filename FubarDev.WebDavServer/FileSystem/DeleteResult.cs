using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem
{
    public class DeleteResult
    {
        public DeleteResult(WebDavStatusCodes statusCode, [CanBeNull] IEntry failedEntry)
        {
            FailedEntry = failedEntry;
            StatusCode = statusCode;
        }

        public WebDavStatusCodes StatusCode { get; }

        [CanBeNull]
        public IEntry FailedEntry { get; }
    }
}
