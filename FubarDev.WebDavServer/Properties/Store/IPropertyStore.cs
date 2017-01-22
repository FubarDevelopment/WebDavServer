using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Properties.Dead;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Properties.Store
{
    public interface IPropertyStore
    {
        int Cost { get; }

        [NotNull]
        IDeadPropertyFactory DeadPropertyFactory { get; }


        Task<XElement> GetAsync(IEntry entry, XName name, CancellationToken cancellationToken);

        Task SetAsync(IEntry entry, XElement element, CancellationToken cancellationToken);

        Task<bool> RemoveAsync(IEntry entry, XName name, CancellationToken cancellationToken);


        Task<IReadOnlyCollection<XElement>> GetAsync(IEntry entry, CancellationToken cancellationToken);

        Task SetAsync(IEntry entry, IEnumerable<XElement> properties, CancellationToken cancellationToken);

        Task<IReadOnlyCollection<bool>> RemoveAsync(IEntry entry, IEnumerable<XName> names, CancellationToken cancellationToken);

        Task RemoveAsync(IEntry entry, CancellationToken cancellationToken);


        Task<IDeadProperty> LoadAsync(IEntry entry, XName name, CancellationToken cancellationToken);

        Task<IReadOnlyCollection<IDeadProperty>> LoadAsync(IEntry entry, CancellationToken cancellationToken);


        Task<EntityTag> GetETagAsync(IDocument document, CancellationToken cancellationToken);

        Task<EntityTag> UpdateETagAsync(IDocument document, CancellationToken cancellationToken);
    }
}
