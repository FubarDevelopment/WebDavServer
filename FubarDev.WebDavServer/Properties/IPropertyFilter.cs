using System.Collections.Generic;

namespace FubarDev.WebDavServer.Properties
{
    public interface IPropertyFilter
    {
        void Reset();

        bool IsAllowed(IProperty property);

        void NotifyOfSelection(IProperty property);

        IEnumerable<MissingProperty> GetMissingProperties();
    }
}
