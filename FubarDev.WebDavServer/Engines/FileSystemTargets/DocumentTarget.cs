using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Engines.FileSystemTargets
{
    public class DocumentTarget : EntryTarget, IDocumentTarget<CollectionTarget, DocumentTarget, MissingTarget>
    {
        private readonly CollectionTarget _parent;
        private readonly IDocument _document;
        private readonly ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> _targetActions;

        public DocumentTarget(CollectionTarget parent, Uri destinationUrl, IDocument document, ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> targetActions)
            : base(destinationUrl, document)
        {
            _parent = parent;
            _document = document;
            _targetActions = targetActions;
        }

        public async Task<MissingTarget> DeleteAsync(CancellationToken cancellationToken)
        {
            await _document.DeleteAsync(cancellationToken).ConfigureAwait(false);
            return new MissingTarget(DestinationUrl, Name, _parent, _targetActions);
        }
    }
}
