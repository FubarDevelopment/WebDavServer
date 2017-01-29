using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Properties;

namespace FubarDev.WebDavServer.Engines.Remote
{
    public abstract class RemoteHttpClientTargetActions : RemoteTargetActions
    {
        private readonly HttpClient _client;

        protected RemoteHttpClientTargetActions(IRemoteHttpClientFactory remoteHttpClientFactory)
        {
            _client = remoteHttpClientFactory.Create();
        }

        public override RecursiveTargetBehaviour ExistingTargetBehaviour { get; } = RecursiveTargetBehaviour.Overwrite;

        public override Task<IReadOnlyCollection<XName>> SetPropertiesAsync(RemoteCollectionTarget target, IEnumerable<IUntypedWriteableProperty> properties, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public override Task<IReadOnlyCollection<XName>> SetPropertiesAsync(RemoteDocumentTarget target, IEnumerable<IUntypedWriteableProperty> properties, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public override Task<RemoteCollectionTarget> CreateCollectionAsync(RemoteCollectionTarget targetCollection, string name, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public override Task<ITarget> GetAsync(string name, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public override Task DeleteAsync(RemoteCollectionTarget target, CancellationToken cancellationToken)
        {
            return _client.DeleteAsync(target.DestinationUrl, cancellationToken);
        }

        public override Task DeleteAsync(RemoteDocumentTarget target, CancellationToken cancellationToken)
        {
            return _client.DeleteAsync(target.DestinationUrl, cancellationToken);
        }
    }
}
