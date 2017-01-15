using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Properties
{
    public interface IUntypedReadableProperty : IProperty
    {
        int Cost { get; }

        [NotNull]
        [ItemNotNull]
        Task<XElement> GetXmlValueAsync(CancellationToken ct);
    }
}
