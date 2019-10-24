// <copyright file="FileSystemTarget.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Linq;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Engines.Local
{
    /// <summary>
    /// The local file system implementation of the <see cref="ITarget"/> interface.
    /// </summary>
    public class FileSystemTarget : ITarget
    {
        private readonly ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> _targetActions;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemTarget"/> class.
        /// </summary>
        /// <param name="destinationUrl">The destination URL for this entry.</param>
        /// <param name="document">The underlying document.</param>
        /// <param name="targetActions">The target actions implementation to use.</param>
        public FileSystemTarget(Uri destinationUrl, IDocument document, ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> targetActions)
        {
            DestinationUrl = destinationUrl;
            Document = document;
            Name = document.Name;
            Parent = document.Parent;
            _targetActions = targetActions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemTarget"/> class.
        /// </summary>
        /// <param name="destinationUrl">The destination URL for this entry.</param>
        /// <param name="collection">The underlying collection.</param>
        /// <param name="targetActions">The target actions implementation to use.</param>
        public FileSystemTarget(Uri destinationUrl, ICollection collection, ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> targetActions)
        {
            DestinationUrl = destinationUrl;
            Collection = collection;
            Name = collection.Name;
            Parent = collection.Parent;
            _targetActions = targetActions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemTarget"/> class.
        /// </summary>
        /// <param name="destinationUrl">The destination URL for this entry.</param>
        /// <param name="collection">The parent collection.</param>
        /// <param name="name">The name of the missing target.</param>
        /// <param name="targetActions">The target actions implementation to use.</param>
        public FileSystemTarget(
            Uri destinationUrl,
            ICollection collection,
            string name,
            ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> targetActions)
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

        /// <summary>
        /// Gets the parent collection.
        /// </summary>
        public ICollection? Parent { get; }

        /// <summary>
        /// Gets the underlying document.
        /// </summary>
        /// <remarks>
        /// Might be <see langword="null"/>, when the target is a collection instead.
        /// </remarks>
        public IDocument? Document { get; }

        /// <summary>
        /// Gets the underlying collection.
        /// </summary>
        /// <remarks>
        /// Might be <see langword="null"/>, when the target is a document instead.
        /// </remarks>
        public ICollection? Collection { get; }

        /// <summary>
        /// Creates a new <see cref="FileSystemTarget"/> from a <paramref name="selectionResult"/>.
        /// </summary>
        /// <param name="selectionResult">The selection result to create this <see cref="FileSystemTarget"/> for.</param>
        /// <param name="destinationUri">The destination URL for the element found by the <see cref="IFileSystem.SelectAsync"/>.</param>
        /// <param name="targetActions">The target actions implementation to use.</param>
        /// <returns>The new file system target.</returns>
        public static FileSystemTarget FromSelectionResult(
            SelectionResult selectionResult,
            Uri destinationUri,
            ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> targetActions)
        {
            if (selectionResult.IsMissing)
            {
                if (selectionResult.MissingNames.Count != 1)
                {
                    throw new InvalidOperationException();
                }

                return new FileSystemTarget(destinationUri, selectionResult.Collection, selectionResult.MissingNames.Single(), targetActions);
            }

            if (selectionResult.ResultType == SelectionResultType.FoundCollection)
            {
                return new FileSystemTarget(destinationUri, selectionResult.Collection, targetActions);
            }

            Debug.Assert(selectionResult.Document != null, "selectionResult.Document != null");
            if (selectionResult.Document == null)
            {
                throw new InvalidOperationException("The document was not found. Undefined behavior.");
            }

            return new FileSystemTarget(destinationUri, selectionResult.Document, targetActions);
        }

        /// <summary>
        /// Creates a new collection target.
        /// </summary>
        /// <remarks>
        /// This only works when this <see cref="FileSystemTarget"/> has an underlying collection.
        /// </remarks>
        /// <returns>The new collection target.</returns>
        public CollectionTarget NewCollectionTarget()
        {
            if (Collection == null)
            {
                throw new InvalidOperationException();
            }

            Uri collUrl = DestinationUrl.GetParent();
            var collTarget = Parent == null ? null : CollectionTarget.NewInstance(collUrl, Parent, _targetActions);
            return new CollectionTarget(DestinationUrl, collTarget, Collection, false, _targetActions);
        }

        /// <summary>
        /// Creates a new document target.
        /// </summary>
        /// <remarks>
        /// This only works when this <see cref="FileSystemTarget"/> has an underlying collection.
        /// </remarks>
        /// <returns>The new document target</returns>
        public DocumentTarget NewDocumentTarget()
        {
            if (Document == null || Parent == null)
            {
                throw new InvalidOperationException();
            }

            Uri collUrl = DestinationUrl.GetParent();
            var collTarget = CollectionTarget.NewInstance(collUrl, Parent, _targetActions);
            return new DocumentTarget(collTarget, DestinationUrl, Document, _targetActions);
        }

        /// <summary>
        /// Creates a new missing target.
        /// </summary>
        /// <remarks>
        /// This only works when this <see cref="FileSystemTarget"/> has an underlying collection.
        /// </remarks>
        /// <returns>The new missing target.</returns>
        public MissingTarget NewMissingTarget()
        {
            if (Parent == null)
            {
                throw new InvalidOperationException();
            }

            Uri collUrl = DestinationUrl.GetParent();
            var collTarget = CollectionTarget.NewInstance(collUrl, Parent, _targetActions);
            return new MissingTarget(DestinationUrl, Name, collTarget, _targetActions);
        }
    }
}
