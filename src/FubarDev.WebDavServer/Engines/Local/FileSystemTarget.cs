// <copyright file="FileSystemTarget.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Linq;

using FubarDev.WebDavServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines.Local
{
    public class FileSystemTarget : ITarget
    {
        private readonly ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> _targetActions;

        public FileSystemTarget([NotNull] Uri destinationUrl, [NotNull] IDocument document, [NotNull] ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> targetActions)
        {
            DestinationUrl = destinationUrl;
            Document = document;
            Name = document.Name;
            Parent = document.Parent;
            _targetActions = targetActions;
        }

        public FileSystemTarget([NotNull] Uri destinationUrl, [NotNull] ICollection collection, [NotNull] ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> targetActions)
        {
            DestinationUrl = destinationUrl;
            Collection = collection;
            Name = collection.Name;
            Parent = collection.Parent;
            _targetActions = targetActions;
        }

        public FileSystemTarget(
            [NotNull] Uri destinationUrl,
            [NotNull] ICollection collection,
            [NotNull] string name,
            [NotNull] ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> targetActions)
        {
            DestinationUrl = destinationUrl;
            Parent = collection;
            Name = name;
            _targetActions = targetActions;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public Uri DestinationUrl { get; }

        [CanBeNull]
        public ICollection Parent { get; }

        [CanBeNull]
        public IDocument Document { get; }

        [CanBeNull]
        public ICollection Collection { get; }

        [NotNull]
        public static FileSystemTarget FromSelectionResult(
            [NotNull] SelectionResult selectionResult,
            [NotNull] Uri destinationUri,
            [NotNull] ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> targetActions)
        {
            if (selectionResult.IsMissing)
            {
                if (selectionResult.MissingNames.Count != 1)
                    throw new InvalidOperationException();
                return new FileSystemTarget(destinationUri, selectionResult.Collection, selectionResult.MissingNames.Single(), targetActions);
            }

            if (selectionResult.ResultType == SelectionResultType.FoundCollection)
            {
                return new FileSystemTarget(destinationUri, selectionResult.Collection, targetActions);
            }

            Debug.Assert(selectionResult.Document != null, "selectionResult.Document != null");
            return new FileSystemTarget(destinationUri, selectionResult.Document, targetActions);
        }

        [NotNull]
        public CollectionTarget NewCollectionTarget()
        {
            if (Collection == null)
                throw new InvalidOperationException();
            Uri collUrl = DestinationUrl.OriginalString.EndsWith("/") ? new Uri(DestinationUrl, "..") : new Uri(DestinationUrl, ".");
            var collTarget = Parent == null ? null : CollectionTarget.NewInstance(collUrl, Parent, _targetActions);
            return new CollectionTarget(DestinationUrl, collTarget, Collection, false, _targetActions);
        }

        [NotNull]
        public DocumentTarget NewDocumentTarget()
        {
            if (Document == null || Parent == null)
                throw new InvalidOperationException();
            Uri collUrl = DestinationUrl.OriginalString.EndsWith("/") ? new Uri(DestinationUrl, "..") : new Uri(DestinationUrl, ".");
            var collTarget = CollectionTarget.NewInstance(collUrl, Parent, _targetActions);
            return new DocumentTarget(collTarget, DestinationUrl, Document, _targetActions);
        }

        [NotNull]
        public MissingTarget NewMissingTarget()
        {
            if (Parent == null)
                throw new InvalidOperationException();
            Uri collUrl = DestinationUrl.OriginalString.EndsWith("/") ? new Uri(DestinationUrl, "..") : new Uri(DestinationUrl, ".");
            var collTarget = CollectionTarget.NewInstance(collUrl, Parent, _targetActions);
            return new MissingTarget(DestinationUrl, Name, collTarget, _targetActions);
        }
    }
}
