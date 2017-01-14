using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Properties.Store
{
    public delegate IUntypedWriteableProperty CreatePropertyDelegate(XName name, int cost, IEntry entry, IPropertyStore store);
}
