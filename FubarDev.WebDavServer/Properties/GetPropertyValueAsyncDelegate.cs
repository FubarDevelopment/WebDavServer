using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Properties
{
    public delegate Task<T> GetPropertyValueAsyncDelegate<T>(CancellationToken cancellationToken);
}
