using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Properties.Store;

namespace FubarDev.WebDavServer.Properties.Dead
{
    public interface IDeadPropertyFactory
    {
        IDeadProperty Create(IPropertyStore store, IEntry entry, XName name);
        IDeadProperty Create(IPropertyStore store, IEntry entry, XElement element);
    }
}
