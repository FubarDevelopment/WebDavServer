using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Properties.Filters
{
    public class PropFilter : IPropertyFilter
    {
        private readonly ISet<XName> _selectedProperties;

        private readonly HashSet<XName> _requestedProperties;

        public PropFilter(Prop prop)
        {
            _requestedProperties = new HashSet<XName>(prop.Any.Select(x => x.Name));
            _selectedProperties = new HashSet<XName>();
        }

        public void Reset()
        {
            _selectedProperties.Clear();
        }

        public bool IsAllowed(IProperty property)
        {
            return _requestedProperties.Contains(property.Name);
        }

        public void NotifyOfSelection(IProperty property)
        {
            _selectedProperties.Add(property.Name);
        }

        public IEnumerable<MissingProperty> GetMissingProperties()
        {
            var missingProps = new HashSet<XName>(_requestedProperties);
            missingProps.ExceptWith(_selectedProperties);
            return missingProps.Select(x => new MissingProperty(WebDavStatusCode.NotFound, x));
        }
    }
}
