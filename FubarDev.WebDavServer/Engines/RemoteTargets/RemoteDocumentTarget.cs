using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Properties;

namespace FubarDev.WebDavServer.Engines.RemoteTargets
{
    public class RemoteDocumentTarget : IDocumentTarget<RemoteCollectionTarget, RemoteDocumentTarget, RemoteMissingTarget>
    {
        public Task<RemoteMissingTarget> DeleteAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public string Name { get; }

        public Uri DestinationUrl { get; }

        public Task<IReadOnlyCollection<XName>> SetPropertiesAsync(IEnumerable<IUntypedWriteableProperty> properties, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
