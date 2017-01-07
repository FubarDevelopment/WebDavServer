using System.Xml.Linq;

namespace FubarDev.WebDavServer.Properties.Converters
{
    public class StringConverter : IPropertyConverter<string>
    {
        private readonly IProperty _property;

        public StringConverter(IProperty property)
        {
            _property = property;
        }

        public string FromElement(XElement element)
        {
            return element.Value;
        }

        public XElement ToElement(string value)
        {
            return new XElement(_property.Name, value);
        }
    }
}
