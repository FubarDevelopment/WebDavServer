using System.Xml.Linq;

namespace FubarDev.WebDavServer.Properties
{
    public interface IInitializableProperty : IProperty
    {
        void Init(XElement initialValue);
    }
}
