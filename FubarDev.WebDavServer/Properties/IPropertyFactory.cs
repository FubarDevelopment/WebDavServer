using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Properties.Store
{
    public interface IPropertyFactory
    {
        IUntypedReadableProperty Create(XName name, IEntry entry, IPropertyStore store);
        IUntypedReadableProperty Create(XElement initialValue, IEntry entry, IPropertyStore store);
    }
}
