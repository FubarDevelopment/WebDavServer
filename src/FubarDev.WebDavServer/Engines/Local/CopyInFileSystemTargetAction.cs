// <copyright file="CopyInFileSystemTargetAction.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Engines.Local
{
    public class CopyInFileSystemTargetAction : ITargetActions<CollectionTarget, DocumentTarget, MissingTarget>
    {
        public RecursiveTargetBehaviour ExistingTargetBehaviour { get; } = RecursiveTargetBehaviour.Overwrite;

        public async Task<DocumentTarget> ExecuteAsync(IDocument source, MissingTarget destination, CancellationToken cancellationToken)
        {
            var doc = await source.CopyToAsync(destination.Parent.Collection, destination.Name, cancellationToken).ConfigureAwait(false);
            return new DocumentTarget(destination.Parent, destination.DestinationUrl, doc, this);
        }

        public async Task<ActionResult> ExecuteAsync(IDocument source, DocumentTarget destination, CancellationToken cancellationToken)
        {
            try
            {
                Debug.Assert(destination.Parent != null, "destination.Parent != null");
                await source.CopyToAsync(destination.Parent.Collection, destination.Name, cancellationToken).ConfigureAwait(false);
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

        public Task ExecuteAsync(ICollection source, CollectionTarget destination, CancellationToken cancellationToken)
        {
            return CopyETagAsync(source, destination.Collection, cancellationToken);
        }

        private static async Task CopyETagAsync(IEntry source, IEntry dest, CancellationToken cancellationToken)
        {
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
