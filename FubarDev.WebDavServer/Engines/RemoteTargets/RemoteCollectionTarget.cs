using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Properties;

namespace FubarDev.WebDavServer.Engines.RemoteTargets
{
    public class RemoteCollectionTarget : ICollectionTarget<RemoteCollectionTarget, RemoteDocumentTarget, RemoteMissingTarget>
    {
        public string Name { get; }

        public Uri DestinationUrl { get; }

        public bool Created { get; }

        public Task<IReadOnlyCollection<XName>> SetPropertiesAsync(IEnumerable<IUntypedWriteableProperty> properties, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<RemoteMissingTarget> DeleteAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ITarget> GetAsync(string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public RemoteMissingTarget NewMissing(string name)
        {
            return new RemoteMissingTarget(DestinationUrl.Append(name, false), name);
        }
    }
}
