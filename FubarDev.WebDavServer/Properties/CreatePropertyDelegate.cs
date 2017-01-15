using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Properties
{
    public delegate IUntypedReadableProperty CreatePropertyDelegate(XName name, int cost, IEntry entry, IPropertyStore store);
}
