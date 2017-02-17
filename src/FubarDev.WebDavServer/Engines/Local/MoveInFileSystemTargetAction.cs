// <copyright file="MoveInFileSystemTargetAction.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Engines.Local
{
    public class MoveInFileSystemTargetAction : ITargetActions<CollectionTarget, DocumentTarget, MissingTarget>
    {
        public RecursiveTargetBehaviour ExistingTargetBehaviour { get; } = RecursiveTargetBehaviour.Overwrite;

        public async Task<DocumentTarget> ExecuteAsync(IDocument source, MissingTarget destination, CancellationToken cancellationToken)
        {
            var doc = await source.MoveToAsync(destination.Parent.Collection, destination.Name, cancellationToken).ConfigureAwait(false);
            return new DocumentTarget(destination.Parent, destination.DestinationUrl, doc, this);
        }

        public async Task<ActionResult> ExecuteAsync(IDocument source, DocumentTarget destination, CancellationToken cancellationToken)
        {
            try
            {
                Debug.Assert(destination.Parent != null, "destination.Parent != null");
                await source.MoveToAsync(destination.Parent.Collection, destination.Name, cancellationToken).ConfigureAwait(false);
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

        public async Task ExecuteAsync(ICollection source, CollectionTarget destination, CancellationToken cancellationToken)
        {
            await CopyETagAsync(source, destination.Collection, cancellationToken).ConfigureAwait(false);
            await source.DeleteAsync(cancellationToken).ConfigureAwait(false);
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
