// <copyright file="DocumentTarget.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines.Local
{
    /// <summary>
    /// The local file system document target
    /// </summary>
    public class DocumentTarget : EntryTarget, IDocumentTarget<CollectionTarget, DocumentTarget, MissingTarget>
    {
        private readonly ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> _targetActions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentTarget"/> class.
        /// </summary>
        /// <param name="parent">The parent collection</param>
        /// <param name="destinationUrl">The destination URL for this collection</param>
        /// <param name="document">The underlying document</param>
        /// <param name="targetActions">The target actions implementation to use</param>
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

        /// <summary>
        /// Gets the underlying document
        /// </summary>
        [NotNull]
        public IDocument Document { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="DocumentTarget"/> class.
        /// </summary>
        /// <param name="destinationUrl">The destination URL for this document</param>
        /// <param name="document">The underlying document</param>
        /// <param name="targetActions">The target actions implementation to use</param>
        /// <returns>The created document target object</returns>
        [NotNull]
        public static DocumentTarget NewInstance(
            [NotNull] Uri destinationUrl,
            [NotNull] IDocument document,
            [NotNull] ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> targetActions)
        {
            var collUrl = destinationUrl.GetCollectionUri();
            Debug.Assert(document.Parent != null, "document.Parent != null");
            var collTarget = new CollectionTarget(collUrl, null, document.Parent, false, targetActions);
            var docTarget = new DocumentTarget(collTarget, destinationUrl, document, targetActions);
            return docTarget;
        }

        /// <inheritdoc />
        public async Task<MissingTarget> DeleteAsync(CancellationToken cancellationToken)
        {
            await Document.DeleteAsync(cancellationToken).ConfigureAwait(false);
            return new MissingTarget(DestinationUrl, Name, Parent, _targetActions);
        }
    }
}
