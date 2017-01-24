using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Engines.FileSystemTargets
{
    public class CollectionTarget : EntryTarget, ICollectionTarget<CollectionTarget, DocumentTarget, MissingTarget>
    {
        private readonly CollectionTarget _parent;

        private readonly ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> _targetActions;

        public CollectionTarget(Uri destinationUrl, CollectionTarget parent, ICollection collection, ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> targetActions)
            : base(destinationUrl, collection)
        {
            Collection = collection;
            _parent = parent;
            _targetActions = targetActions;
        }

        public ICollection Collection { get; }

        public async Task<MissingTarget> DeleteAsync(CancellationToken cancellationToken)
        {
            var name = Collection.Name;
            await Collection.DeleteAsync(cancellationToken).ConfigureAwait(false);
            return new MissingTarget(DestinationUrl, name, _parent, _targetActions);
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
            return new CollectionTarget(DestinationUrl.Append(coll), this, coll, _targetActions);
        }

        public MissingTarget CreateMissing(string name)
        {
            return new MissingTarget(DestinationUrl.Append(name, false), name, this, _targetActions);
        }
    }
}
