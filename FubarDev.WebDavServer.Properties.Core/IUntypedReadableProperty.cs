using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.Properties
{
    public interface IUntypedReadableProperty : IProperty
    {
        int Cost { get; }
        Task<XElement> GetXmlValueAsync(CancellationToken ct);
    }
}
