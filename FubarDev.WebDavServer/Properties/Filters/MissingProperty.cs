using System.Xml.Linq;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Properties.Filters
{
    public class MissingProperty
    {
        public MissingProperty(WebDavStatusCodes statusCode, XName name)
        {
            StatusCode = statusCode;
            PropertyName = name;
        }

        public WebDavStatusCodes StatusCode { get; }

        public XName PropertyName { get; }
    }
}
