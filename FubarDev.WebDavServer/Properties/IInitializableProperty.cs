using System.Xml.Linq;

namespace FubarDev.WebDavServer.Properties
{
    public interface IInitializableProperty
    {
        void Init(XElement initialValue);
    }
}
