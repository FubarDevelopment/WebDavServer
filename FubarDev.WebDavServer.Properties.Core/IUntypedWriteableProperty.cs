using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.Properties
{
    public interface IUntypedWriteableProperty : IProperty
    {
        Task SetXmlValueAsync(XElement element, CancellationToken ct);
    }
}
