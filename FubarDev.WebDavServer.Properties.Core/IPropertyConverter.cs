using System.Xml.Linq;

namespace FubarDev.WebDavServer.Properties
{
    public interface IPropertyConverter<T>
    {
        T FromElement(XElement element);
        XElement ToElement(T value);
    }
}
