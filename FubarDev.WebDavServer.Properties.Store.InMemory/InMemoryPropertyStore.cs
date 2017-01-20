using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Properties.Events;

namespace FubarDev.WebDavServer.Properties.Store.InMemory
{
    public class InMemoryPropertyStore : IPropertyStore
    {
        private IDictionary<Uri, IDictionary<XName, IUntypedReadableProperty>> _properties = new Dictionary<Uri, IDictionary<XName, IUntypedReadableProperty>>();

        public int Cost { get; } = 0;

        public Task<IReadOnlyCollection<IUntypedReadableProperty>> LoadAndCreateAsync(IEntry entry, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<XElement> LoadRawAsync(IEntry entry, XName name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SaveRawAsync(IEntry entry, XElement element, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(IEntry entry, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveRawAsync(IEntry entry, XName name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<EntityTag> GetETagAsync(IDocument document, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<EntityTag> UpdateETagAsync(IDocument document, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task HandleMovedEntryAsync(EntryMoved info, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task HandleModifiedEntryAsync(IEntry entry, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
