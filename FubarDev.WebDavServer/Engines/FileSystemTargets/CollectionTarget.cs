using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Engines.FileSystemTargets
{
    public class CollectionTarget : EntryTarget, ICollectionTarget
    {
        private readonly ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> _targetActions;

        public CollectionTarget(Uri destinationUrl, ICollection collection, ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> targetActions)
            : base(destinationUrl, collection)
        {
            Collection = collection;
            _targetActions = targetActions;
        }

        public ICollection Collection { get; }

        public Task<ExecutionResult> CreateAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new ExecutionResult()
            {
                StatusCode = WebDavStatusCodes.OK,
                Href = DestinationUrl,
            });
        }

        public async Task<ITarget> GetAsync(string name, CancellationToken cancellationToken)
        {
            var result = await Collection.GetChildAsync(name, cancellationToken).ConfigureAwait(false);
            if (result == null)
                return null;
            var doc = result as IDocument;
            if (doc != null)
                return new DocumentTarget(this, DestinationUrl.Append(doc), doc, _targetActions);

            var coll = (ICollection) result;
            return new CollectionTarget(DestinationUrl.Append(coll), coll, _targetActions);
        }
    }
}
