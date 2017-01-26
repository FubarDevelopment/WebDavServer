using System;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Engines.RemoteTargets
{
    public class RemoteMissingTarget : IMissingTarget<RemoteCollectionTarget, RemoteDocumentTarget, RemoteMissingTarget>
    {
        public RemoteMissingTarget(Uri destinationUrl, string name)
        {
            Name = name;
            DestinationUrl = destinationUrl;
        }

        public string Name { get; }

        public Uri DestinationUrl { get; }

        public Task<RemoteCollectionTarget> CreateCollectionAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
