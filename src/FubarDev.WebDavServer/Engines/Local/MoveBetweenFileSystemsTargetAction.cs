// <copyright file="MoveBetweenFileSystemsTargetAction.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Engines.Local
{
    /// <summary>
    /// The <see cref="ITargetActions{TCollection,TDocument,TMissing}"/> implementation that moves between two file systems.
    /// </summary>
    public class MoveBetweenFileSystemsTargetAction : ITargetActions<CollectionTarget, DocumentTarget, MissingTarget>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MoveBetweenFileSystemsTargetAction"/> class.
        /// </summary>
        /// <param name="context">The current WebDAV context.</param>
        public MoveBetweenFileSystemsTargetAction(IWebDavContext context)
        {
            Context = context;
        }

        /// <inheritdoc />
        public IWebDavContext Context { get; }

        /// <inheritdoc />
        public RecursiveTargetBehaviour ExistingTargetBehaviour { get; } = RecursiveTargetBehaviour.Overwrite;

        /// <inheritdoc />
        public async Task<DocumentTarget> ExecuteAsync(IDocument source, MissingTarget destination, CancellationToken cancellationToken)
        {
            var doc = await destination.Parent.Collection.CreateDocumentAsync(destination.Name, cancellationToken).ConfigureAwait(false);
            var docTarget = new DocumentTarget(destination.Parent, destination.DestinationUrl, doc, this);
            await MoveAsync(source, doc, cancellationToken).ConfigureAwait(false);
            await CopyETagAsync(source, doc, cancellationToken).ConfigureAwait(false);
            await source.DeleteAsync(cancellationToken).ConfigureAwait(false);

            return docTarget;
        }

        /// <inheritdoc />
        public async Task<ActionResult> ExecuteAsync(IDocument source, DocumentTarget destination, CancellationToken cancellationToken)
        {
            try
            {
                await MoveAsync(source, destination.Document, cancellationToken).ConfigureAwait(false);
                await CopyETagAsync(source, destination.Document, cancellationToken).ConfigureAwait(false);
                await source.DeleteAsync(cancellationToken).ConfigureAwait(false);

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
        public async Task ExecuteAsync(ICollection source, CollectionTarget destination, CancellationToken cancellationToken)
        {
            await CopyETagAsync(source, destination.Collection, cancellationToken).ConfigureAwait(false);
            await source.DeleteAsync(cancellationToken).ConfigureAwait(false);
        }

        private static async Task MoveAsync(IDocument source, IDocument destination, CancellationToken cancellationToken)
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
            {
                return;
            }

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
