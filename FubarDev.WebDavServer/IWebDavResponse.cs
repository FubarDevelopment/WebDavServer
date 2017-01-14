using System.Collections.Generic;
using System.IO;

namespace FubarDev.WebDavServer
{
    public interface IWebDavResponse
    {
        IWebDavDispatcher Dispatcher { get; }

        IDictionary<string, string[]> Headers { get; }

        string ContentType { get; set; }

        Stream Body { get; }
    }
}
