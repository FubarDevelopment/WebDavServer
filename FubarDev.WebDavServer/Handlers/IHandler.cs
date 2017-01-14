using System.Collections.Generic;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IHandler
    {
        IEnumerable<string> HttpMethods { get; }
    }
}
