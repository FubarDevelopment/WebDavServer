using System.Collections.Generic;
using System.Linq;

namespace FubarDev.WebDavServer.Properties.Filters
{
    public class ReadableFilter : IPropertyFilter
    {
        public void Reset()
        {
        }

        public bool IsAllowed(IProperty property)
        {
            return property is IUntypedReadableProperty;
        }

        public void NotifyOfSelection(IProperty property)
        {
        }

        public IEnumerable<MissingProperty> GetMissingProperties()
        {
            return Enumerable.Empty<MissingProperty>();
        }
    }
}
