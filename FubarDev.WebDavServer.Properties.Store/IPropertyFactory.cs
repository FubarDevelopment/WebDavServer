using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Properties.Store
{
    public interface IPropertyFactory
    {
        IUntypedWriteableProperty Create(XName name, IEntry entry, IPropertyStore store);
        IUntypedWriteableProperty Create(XElement initialValue, IEntry entry, IPropertyStore store);
    }
}
