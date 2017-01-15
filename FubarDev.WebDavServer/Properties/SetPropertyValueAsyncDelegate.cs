using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Properties
{
    public delegate Task SetPropertyValueAsyncDelegate<T>(T value, CancellationToken cancellationToken);
}
