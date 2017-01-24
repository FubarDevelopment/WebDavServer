using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Engines.FileSystemTargets
{
    public class DocumentTarget : EntryTarget, IDocumentTarget
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

        public async Task<ExecutionResult> ExecuteAsync(Uri sourceUrl, IDocument source, CancellationToken cancellationToken)
        {
            if (_targetActions.ExistingTargetBehaviour == RecursiveTargetBehaviour.DeleteBeforeCopy)
            {
                await _document.DeleteAsync(cancellationToken).ConfigureAwait(false);
                var missingTarget = new MissingTarget(DestinationUrl, Name, _parent, _targetActions);
                return await missingTarget.ExecuteAsync(sourceUrl, source, cancellationToken).ConfigureAwait(false);
            }

            await _targetActions.ExecuteAsync(source, this, cancellationToken).ConfigureAwait(false);

            return new ExecutionResult()
            {
                Target = this,
                Href = DestinationUrl,
                StatusCode = WebDavStatusCodes.OK
            };
        }
    }
}
