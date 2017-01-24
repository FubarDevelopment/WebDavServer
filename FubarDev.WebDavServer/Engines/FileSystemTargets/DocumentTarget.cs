using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Engines.FileSystemTargets
{
    public class DocumentTarget : EntryTarget, IDocumentTarget<CollectionTarget, DocumentTarget, MissingTarget>
    {
        private readonly ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> _targetActions;

        public DocumentTarget(CollectionTarget parent, Uri destinationUrl, IDocument document, ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> targetActions)
            : base(destinationUrl, document)
        {
            Parent = parent;
            _targetActions = targetActions;
            Document = document;
        }

        public IDocument Document { get; }

        public CollectionTarget Parent { get; }

        public async Task<MissingTarget> DeleteAsync(CancellationToken cancellationToken)
        {
            await Document.DeleteAsync(cancellationToken).ConfigureAwait(false);
            return new MissingTarget(DestinationUrl, Name, Parent, _targetActions);
        }
    }
}
