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

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props.Store
{
    public abstract class PropertyStoreBase : IPropertyStore
    {
        protected PropertyStoreBase([NotNull] IDeadPropertyFactory deadPropertyFactory)
        {
            DeadPropertyFactory = deadPropertyFactory;
        }

        public abstract int Cost { get; }

        [NotNull]
        protected IDeadPropertyFactory DeadPropertyFactory { get; }

        public virtual async Task<XElement> GetAsync(IEntry entry, XName name, CancellationToken cancellationToken)
        {
            var elements = await GetAsync(entry, cancellationToken).ConfigureAwait(false);
            return elements.FirstOrDefault(x => x.Name == name);
        }

        public virtual Task SetAsync(IEntry entry, XElement element, CancellationToken cancellationToken)
        {
            return SetAsync(entry, new[] { element }, cancellationToken);
        }

        public virtual async Task<bool> RemoveAsync(IEntry entry, XName name, CancellationToken cancellationToken)
        {
            return (await RemoveAsync(entry, new[] { name }, cancellationToken).ConfigureAwait(false)).Single();
        }

        public abstract Task<IReadOnlyCollection<XElement>> GetAsync(IEntry entry, CancellationToken cancellationToken);

        public abstract Task SetAsync(IEntry entry, IEnumerable<XElement> properties, CancellationToken cancellationToken);

        public abstract Task<IReadOnlyCollection<bool>> RemoveAsync(IEntry entry, IEnumerable<XName> names, CancellationToken cancellationToken);

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

        public IDeadProperty Create(IEntry entry, XName name)
        {
            return DeadPropertyFactory.Create(this, entry, name);
        }

        public virtual async Task<IDeadProperty> LoadAsync(IEntry entry, XName name, CancellationToken cancellationToken)
        {
            var element = await GetAsync(entry, name, cancellationToken).ConfigureAwait(false);
            if (element == null)
                return Create(entry, name);
            return CreateProperty(entry, element);
        }

        public virtual async Task<IReadOnlyCollection<IDeadProperty>> LoadAsync(IEntry entry, CancellationToken cancellationToken)
        {
            var elements = await GetAsync(entry, cancellationToken).ConfigureAwait(false);
            return elements.Select(x => CreateProperty(entry, x)).ToList();
        }

        public Task<EntityTag> GetETagAsync(IEntry entry, CancellationToken cancellationToken)
        {
            var etagEntry = entry as IEntityTagEntry;
            if (etagEntry != null)
                return Task.FromResult(etagEntry.ETag);

            return GetDeadETagAsync(entry, cancellationToken);
        }

        public Task<EntityTag> UpdateETagAsync(IEntry entry, CancellationToken cancellationToken)
        {
            var etagEntry = entry as IEntityTagEntry;
            if (etagEntry != null)
                return etagEntry.UpdateETagAsync(cancellationToken);

            return UpdateETagAsync(entry, cancellationToken);
        }

        protected abstract Task<EntityTag> GetDeadETagAsync(IEntry entry, CancellationToken cancellationToken);

        protected abstract Task<EntityTag> UpdateDeadETagAsync(IEntry entry, CancellationToken cancellationToken);

        private IDeadProperty CreateProperty(IEntry entry, XElement element)
        {
            return DeadPropertyFactory.Create(this, entry, element);
        }
    }
}
