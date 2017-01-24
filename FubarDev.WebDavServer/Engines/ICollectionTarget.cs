using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines
{
    public interface ICollectionTarget : IExistingTarget
    {
        [NotNull, ItemCanBeNull]
        Task<ITarget> GetAsync([NotNull] string name, CancellationToken cancellationToken);
    }
}
