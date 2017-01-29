using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Engines.Local
{
    public class CopyBetweenFileSystemsTargetAction : ITargetActions<CollectionTarget, DocumentTarget, MissingTarget>
    {
        public RecursiveTargetBehaviour ExistingTargetBehaviour { get; } = RecursiveTargetBehaviour.Overwrite;

        public async Task<DocumentTarget> ExecuteAsync(IDocument source, MissingTarget destination, CancellationToken cancellationToken)
        {
            var doc = await destination.Parent.Collection.CreateDocumentAsync(destination.Name, cancellationToken).ConfigureAwait(false);
            var docTarget = new DocumentTarget(destination.Parent, destination.DestinationUrl, doc, this);
            var result = await ExecuteAsync(source, docTarget, cancellationToken).ConfigureAwait(false);
            if (result.IsFailure)
                throw new Exception(result.Exception.Message, result.Exception);
            return docTarget;
        }

        public async Task<ActionResult> ExecuteAsync(IDocument source, DocumentTarget destination, CancellationToken cancellationToken)
        {
            try
            {
                using (var sourceStream = await source.OpenReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    using (var destinationStream = await destination.Document.CreateAsync(cancellationToken).ConfigureAwait(false))
                    {
                        await sourceStream.CopyToAsync(destinationStream, 65536, cancellationToken).ConfigureAwait(false);
                    }
                }
                return new ActionResult(ActionStatus.Overwritten, destination);
            }
            catch (Exception ex)
            {
                return new ActionResult(ActionStatus.OverwriteFailed, destination)
                {
                    Exception = ex,
                };
            }
        }

        public Task ExecuteAsync(ICollection source, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}
