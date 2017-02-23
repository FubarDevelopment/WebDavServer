// <copyright file="CollectionTarget.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines.Local
{
    /// <summary>
    /// The local file system collection target
    /// </summary>
    public class CollectionTarget : EntryTarget, ICollectionTarget<CollectionTarget, DocumentTarget, MissingTarget>
    {
        [NotNull]
        private readonly ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> _targetActions;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionTarget"/> class.
        /// </summary>
        /// <param name="destinationUrl">The destination URL for this collection</param>
        /// <param name="parent">The parent collection</param>
        /// <param name="collection">The underlying collection</param>
        /// <param name="created">Was this collection created by the <see cref="RecursiveExecutionEngine{TCollection,TDocument,TMissing}"/></param>
        /// <param name="targetActions">The target actions implementation to use</param>
        public CollectionTarget(
            [NotNull] Uri destinationUrl,
            [CanBeNull] CollectionTarget parent,
            [NotNull] ICollection collection,
            bool created,
            [NotNull] ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> targetActions)
            : base(parent, destinationUrl, collection)
        {
            Collection = collection;
            _targetActions = targetActions;
            Created = created;
        }

        /// <summary>
        /// Gets the underlying collection
        /// </summary>
        [NotNull]
        public ICollection Collection { get; }

        /// <inheritdoc />
        public bool Created { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="CollectionTarget"/> class.
        /// </summary>
        /// <param name="destinationUrl">The destination URL for this collection</param>
        /// <param name="collection">The underlying collection</param>
        /// <param name="targetActions">The target actions implementation to use</param>
        /// <returns>The created collection target object</returns>
        [NotNull]
        public static CollectionTarget NewInstance(
            [NotNull] Uri destinationUrl,
            [NotNull] ICollection collection,
            [NotNull] ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> targetActions)
        {
            CollectionTarget parentTarget;
            if (collection.Parent != null)
            {
                var collUrl = destinationUrl.GetParent();
                parentTarget = new CollectionTarget(collUrl, null, collection.Parent, false, targetActions);
            }
            else
            {
                parentTarget = null;
            }

            var target = new CollectionTarget(destinationUrl, parentTarget, collection, false, targetActions);
            return target;
        }

        /// <inheritdoc />
        public async Task<MissingTarget> DeleteAsync(CancellationToken cancellationToken)
        {
            var name = Collection.Name;
            await Collection.DeleteAsync(cancellationToken).ConfigureAwait(false);
            return new MissingTarget(DestinationUrl, name, Parent, _targetActions);
        }

        /// <inheritdoc />
        public async Task<ITarget> GetAsync(string name, CancellationToken cancellationToken)
        {
            var result = await Collection.GetChildAsync(name, cancellationToken).ConfigureAwait(false);
            if (result == null)
                return new MissingTarget(DestinationUrl.Append(name, false), name, this, _targetActions);

            var doc = result as IDocument;
            if (doc != null)
                return new DocumentTarget(this, DestinationUrl.Append(doc), doc, _targetActions);

            var coll = (ICollection)result;
            return new CollectionTarget(DestinationUrl.Append(coll), this, coll, false, _targetActions);
        }

        /// <inheritdoc />
        public MissingTarget NewMissing(string name)
        {
            return new MissingTarget(DestinationUrl.Append(name, false), name, this, _targetActions);
        }
    }
}
