// <copyright file="CopyBetweenFileSystemsTargetAction.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Engines.Local
{
    /// <summary>
    /// The <see cref="ITargetActions{TCollection,TDocument,TMissing}"/> implementation that copies between two file systems
    /// </summary>
    public class CopyBetweenFileSystemsTargetAction : ITargetActions<CollectionTarget, DocumentTarget, MissingTarget>
    {
        /// <inheritdoc />
        public RecursiveTargetBehaviour ExistingTargetBehaviour { get; } = RecursiveTargetBehaviour.Overwrite;

        /// <inheritdoc />
        public async Task<DocumentTarget> ExecuteAsync(IDocument source, MissingTarget destination, CancellationToken cancellationToken)
        {
            var doc = await destination.Parent.Collection.CreateDocumentAsync(destination.Name, cancellationToken).ConfigureAwait(false);

            var docTarget = new DocumentTarget(destination.Parent, destination.DestinationUrl, doc, this);
            await CopyAsync(source, doc, cancellationToken).ConfigureAwait(false);
            await CopyETagAsync(source, doc, cancellationToken).ConfigureAwait(false);

            return docTarget;
        }

        /// <inheritdoc />
        public async Task<ActionResult> ExecuteAsync(IDocument source, DocumentTarget destination, CancellationToken cancellationToken)
        {
            try
            {
                await CopyAsync(source, destination.Document, cancellationToken).ConfigureAwait(false);
                await CopyETagAsync(source, destination.Document, cancellationToken).ConfigureAwait(false);
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

        /// <inheritdoc />
        public Task ExecuteAsync(ICollection source, CollectionTarget destination, CancellationToken cancellationToken)
        {
            return CopyETagAsync(source, destination.Collection, cancellationToken);
        }

        private static async Task CopyAsync(IDocument source, IDocument destination, CancellationToken cancellationToken)
        {
            using (var sourceStream = await source.OpenReadAsync(cancellationToken).ConfigureAwait(false))
            {
                using (var destinationStream = await destination.CreateAsync(cancellationToken).ConfigureAwait(false))
                {
                    await sourceStream.CopyToAsync(destinationStream, 65536, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private static async Task CopyETagAsync(IEntry source, IEntry dest, CancellationToken cancellationToken)
        {
            if (dest is IEntityTagEntry)
                return;

            var sourcePropStore = source.FileSystem.PropertyStore;
            var destPropStore = dest.FileSystem.PropertyStore;
            if (sourcePropStore != null && destPropStore != null)
            {
                var etag = await sourcePropStore.GetETagAsync(source, cancellationToken).ConfigureAwait(false);
                await destPropStore.SetAsync(dest, etag.ToXml(), cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
