using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Properties.Store.Events;

namespace FubarDev.WebDavServer.Properties.Store
{
    public interface IPropertyStore
    {
        int Cost { get; }

        Task<IReadOnlyCollection<IUntypedReadableProperty>> LoadAndCreateAsync(IEntry entry, CancellationToken cancellationToken);

        Task<XElement> LoadRawAsync(IEntry entry, XName name, CancellationToken cancellationToken);

        Task SaveRawAsync(IEntry entry, XElement element, CancellationToken cancellationToken);

        Task RemoveRawAsync(IEntry entry, XName name, CancellationToken cancellationToken);

        Task<EntityTag> GetETagAsync(IDocument document, CancellationToken cancellationToken);

        Task<EntityTag> UpdateETagAsync(IDocument document, CancellationToken cancellationToken);

        Task HandleMovedEntryAsync(EntryMoved info, CancellationToken ct);

        Task HandleModifiedEntryAsync(IEntry entry, CancellationToken ct);
    }
}
