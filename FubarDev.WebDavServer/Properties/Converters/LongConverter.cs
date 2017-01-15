using System.Xml;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.Properties.Converters
{
    public class LongConverter : IPropertyConverter<long>
    {
        public long FromElement(XElement element)
        {
            return XmlConvert.ToInt64(element.Value);
        }

        public XElement ToElement(XName name, long value)
        {
            return new XElement(name, XmlConvert.ToString(value));
        }
    }
}
