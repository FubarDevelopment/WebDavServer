using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Properties
{
    public interface IProperty
    {
        [NotNull]
        XName Name { get; }
    }
}
