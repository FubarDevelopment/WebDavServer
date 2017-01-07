using System.Xml.Linq;

namespace FubarDev.WebDavServer.Properties
{
    public interface IProperty
    {
        XName Name { get; }
    }
}
