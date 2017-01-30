using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.AspNetCore
{
    public class LoggingWebDavResponse : IWebDavResponse
    {
        public LoggingWebDavResponse(IWebDavDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
        }

        public IWebDavDispatcher Dispatcher { get; }
        public IDictionary<string, string[]> Headers { get; } = new Dictionary<string, string[]>();
        public string ContentType { get; set; }
        public Stream Body { get; } = new MemoryStream();

        public XDocument Load()
        {
            Body.Position = 0;
            return XDocument.Load(Body);
        }
    }
}
