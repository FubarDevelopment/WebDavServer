// <copyright file="DocumentTarget.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines.Local
{
    public class DocumentTarget : EntryTarget, IDocumentTarget<CollectionTarget, DocumentTarget, MissingTarget>
    {
        private readonly ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> _targetActions;

        public DocumentTarget(
            [NotNull] CollectionTarget parent,
            [NotNull] Uri destinationUrl,
            [NotNull] IDocument document,
            [NotNull] ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> targetActions)
            : base(parent, destinationUrl, document)
        {
            _targetActions = targetActions;
            Document = document;
        }

        [NotNull]
        public IDocument Document { get; }

        [NotNull]
        public static DocumentTarget NewInstance(
            [NotNull] Uri destinationUrl,
            [NotNull] IDocument document,
            [NotNull] ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> targetActions)
        {
            var collUrl = new Uri(destinationUrl, new Uri(".", UriKind.Relative));
            var collTarget = new CollectionTarget(collUrl, null, document.Parent, false, targetActions);
            var docTarget = new DocumentTarget(collTarget, destinationUrl, document, targetActions);
            return docTarget;
        }

        public async Task<MissingTarget> DeleteAsync(CancellationToken cancellationToken)
        {
            await Document.DeleteAsync(cancellationToken).ConfigureAwait(false);
            return new MissingTarget(DestinationUrl, Name, Parent, _targetActions);
        }
    }
}
