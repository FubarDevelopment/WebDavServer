using System.Collections.Generic;
using System.Linq;

using FubarDev.WebDavServer.Dispatchers;
using FubarDev.WebDavServer.Formatters;

using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer
{
    public class WebDavServer : IWebDavDispatcher
    {
        public WebDavServer(IWebDavClass1 webDavClass1, IWebDavOutputFormatter formatter)
        {
            Formatter = formatter;
            Class1 = webDavClass1;
            var classes = new IWebDavClass[] { webDavClass1 }.Where(x => x != null).ToList();
            SupportedClasses = classes.Select(x => x.Version).ToList();
            SupportedHttpMethods = classes.SelectMany(x => x.HttpMethods).ToList();
        }

        public IReadOnlyCollection<string> SupportedHttpMethods { get; }

        public IReadOnlyCollection<int> SupportedClasses { get; }

        public IWebDavOutputFormatter Formatter { get; }

        public IWebDavClass1 Class1 { get; }
    }
}
