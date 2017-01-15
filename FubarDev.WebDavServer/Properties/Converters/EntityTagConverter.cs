using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using FubarDev.WebDavServer.Properties.Store;

namespace FubarDev.WebDavServer.Properties.Converters
{
    public class EntityTagConverter : IPropertyConverter<EntityTag>
    {
        public EntityTag FromElement(XElement element)
        {
            return EntityTag.FromXml(element);
        }

        public XElement ToElement(XName name, EntityTag value)
        {
            return value.ToXml();
        }
    }
}
