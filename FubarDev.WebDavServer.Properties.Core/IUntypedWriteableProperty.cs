using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Properties
{
    public interface IUntypedWriteableProperty : IProperty
    {
        [NotNull]
        Task SetXmlValueAsync([NotNull] XElement element, CancellationToken ct);
    }
}
