// <copyright file="PropertyStoreBase.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.Props.Dead;

namespace FubarDev.WebDavServer.Props.Store
{
    /// <summary>
    /// Common functionality for a property store implementation.
    /// </summary>
    public abstract class PropertyStoreBase : IPropertyStore
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyStoreBase"/> class.
        /// </summary>
        /// <param name="deadPropertyFactory">The factory to create dead properties.</param>
        protected PropertyStoreBase(IDeadPropertyFactory deadPropertyFactory)
        {
            DeadPropertyFactory = deadPropertyFactory;
        }

        /// <inheritdoc />
        public abstract int Cost { get; }

        /// <summary>
        /// Gets the dead property factory.
        /// </summary>
        protected IDeadPropertyFactory DeadPropertyFactory { get; }

        /// <inheritdoc />
        public virtual async Task<XElement?> GetAsync(IEntry entry, XName name, CancellationToken cancellationToken)
        {
            var elements = await GetAsync(entry, cancellationToken).ConfigureAwait(false);
            return elements.FirstOrDefault(x => x.Name == name);
        }

        /// <inheritdoc />
        public virtual Task SetAsync(IEntry entry, XElement element, CancellationToken cancellationToken)
        {
            return SetAsync(entry, new[] { element }, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<bool> RemoveAsync(IEntry entry, XName name, CancellationToken cancellationToken)
        {
            return (await RemoveAsync(entry, new[] { name }, cancellationToken).ConfigureAwait(false)).Single();
        }

        /// <inheritdoc />
        public abstract Task<IReadOnlyCollection<XElement>> GetAsync(IEntry entry, CancellationToken cancellationToken);

        /// <inheritdoc />
        public abstract Task SetAsync(IEntry entry, IEnumerable<XElement> properties, CancellationToken cancellationToken);

        /// <inheritdoc />
        public abstract Task<IReadOnlyCollection<bool>> RemoveAsync(IEntry entry, IEnumerable<XName> names, CancellationToken cancellationToken);

        /// <inheritdoc />
        public virtual async Task RemoveAsync(IEntry entry, CancellationToken cancellationToken)
        {
            var elements = await GetAsync(entry, cancellationToken).ConfigureAwait(false);
            var names = elements.Where(x => x.Name != GetETagProperty.PropertyName).Select(x => x.Name).ToList();
            if (elements.Count != names.Count)
            {
                // Has ETag, so force the update of an ETag
                await UpdateETagAsync(entry, cancellationToken).ConfigureAwait(false);
            }

            await RemoveAsync(entry, names, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public IDeadProperty Create(IEntry entry, XName name)
        {
            return DeadPropertyFactory.Create(this, entry, name);
        }

        /// <inheritdoc />
        public virtual async Task<IDeadProperty> LoadAsync(IEntry entry, XName name, CancellationToken cancellationToken)
        {
            var element = await GetAsync(entry, name, cancellationToken).ConfigureAwait(false);
            if (element == null)
            {
                return Create(entry, name);
            }

            return CreateProperty(entry, element);
        }

        /// <inheritdoc />
        public virtual async Task<IReadOnlyCollection<IDeadProperty>> LoadAsync(IEntry entry, CancellationToken cancellationToken)
        {
            var elements = await GetAsync(entry, cancellationToken).ConfigureAwait(false);
            return elements.Select(x => CreateProperty(entry, x)).ToList();
        }

        /// <inheritdoc />
        public Task<EntityTag> GetETagAsync(IEntry entry, CancellationToken cancellationToken)
        {
            var etagEntry = entry as IEntityTagEntry;
            if (etagEntry != null)
            {
                return Task.FromResult(etagEntry.ETag);
            }

            return GetDeadETagAsync(entry, cancellationToken);
        }

        /// <inheritdoc />
        public Task<EntityTag> UpdateETagAsync(IEntry entry, CancellationToken cancellationToken)
        {
            var etagEntry = entry as IEntityTagEntry;
            if (etagEntry != null)
            {
                return etagEntry.UpdateETagAsync(cancellationToken);
            }

            return UpdateDeadETagAsync(entry, cancellationToken);
        }

        /// <summary>
        /// Gets a <see cref="GetETagProperty"/> from the property store.
        /// </summary>
        /// <param name="entry">The entry to get the <c>getetag</c> property from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The entity tag for the <paramref name="entry"/>.</returns>
        protected abstract Task<EntityTag> GetDeadETagAsync(IEntry entry, CancellationToken cancellationToken);

        /// <summary>
        /// Updates a <see cref="GetETagProperty"/> in the property store.
        /// </summary>
        /// <param name="entry">The entry to update the <c>getetag</c> property for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated entity tag for the <paramref name="entry"/>.</returns>
        protected abstract Task<EntityTag> UpdateDeadETagAsync(IEntry entry, CancellationToken cancellationToken);

        private IDeadProperty CreateProperty(IEntry entry, XElement element)
        {
            return DeadPropertyFactory.Create(this, entry, element);
        }
    }
}
