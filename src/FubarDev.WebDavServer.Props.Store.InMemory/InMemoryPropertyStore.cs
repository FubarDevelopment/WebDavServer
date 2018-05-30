// <copyright file="InMemoryPropertyStore.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.Props.Dead;

using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Props.Store.InMemory
{
    /// <summary>
    /// The in-memory implementation of a property store
    /// </summary>
    public class InMemoryPropertyStore : PropertyStoreBase
    {
        private readonly ILogger<InMemoryPropertyStore> _logger;
        private readonly IDictionary<Uri, IDictionary<XName, XElement>> _properties = new Dictionary<Uri, IDictionary<XName, XElement>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryPropertyStore"/> class.
        /// </summary>
        /// <param name="deadPropertyFactory">The factory to create dead properties</param>
        /// <param name="logger">The logger</param>
        public InMemoryPropertyStore(IDeadPropertyFactory deadPropertyFactory, ILogger<InMemoryPropertyStore> logger)
            : base(deadPropertyFactory)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public override int Cost { get; } = 0;

        /// <inheritdoc />
        public override Task<IReadOnlyCollection<XElement>> GetAsync(IEntry entry, CancellationToken cancellationToken)
        {
            var entries = GetAll(entry)
                .Where(x => x.Name != GetETagProperty.PropertyName)
                .ToList();
            return Task.FromResult<IReadOnlyCollection<XElement>>(entries);
        }

        /// <inheritdoc />
        public override Task SetAsync(IEntry entry, IEnumerable<XElement> elements, CancellationToken cancellationToken)
        {
            var elementsToSet = new List<XElement>();
            foreach (var element in elements)
            {
                if (element.Name == GetETagProperty.PropertyName)
                {
                    _logger.LogWarning("The ETag property must not be set using the property store.");
                    continue;
                }

                elementsToSet.Add(element);
            }

            SetAll(entry, elementsToSet);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task RemoveAsync(IEntry entry, CancellationToken cancellationToken)
        {
            _properties.Remove(entry.Path);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task<IReadOnlyCollection<bool>> RemoveAsync(IEntry entry, IEnumerable<XName> keys, CancellationToken cancellationToken)
        {
            var result = new List<bool>();
            IDictionary<XName, XElement> properties;
            if (!_properties.TryGetValue(entry.Path, out properties))
            {
                result.AddRange(keys.Select(x => false));
            }
            else
            {
                foreach (var key in keys)
                {
                    if (key == GetETagProperty.PropertyName)
                    {
                        _logger.LogWarning("The ETag property must not be set using the property store.");
                        result.Add(false);
                    }
                    else
                    {
                        result.Add(properties.Remove(key));
                    }
                }

                if (properties.Count == 0)
                    _properties.Remove(entry.Path);
            }

            return Task.FromResult<IReadOnlyCollection<bool>>(result);
        }

        /// <inheritdoc />
        protected override Task<EntityTag> GetDeadETagAsync(IEntry entry, CancellationToken cancellationToken)
        {
            XElement etagElement;
            IDictionary<XName, XElement> properties;
            if (_properties.TryGetValue(entry.Path, out properties))
            {
                properties.TryGetValue(GetETagProperty.PropertyName, out etagElement);
            }
            else
            {
                etagElement = null;
            }

            if (etagElement == null)
            {
                etagElement = new EntityTag(false).ToXml();
                _properties.Add(entry.Path, new Dictionary<XName, XElement>()
                {
                    [etagElement.Name] = etagElement,
                });
            }

            return Task.FromResult(EntityTag.FromXml(etagElement));
        }

        /// <inheritdoc />
        protected override Task<EntityTag> UpdateDeadETagAsync(IEntry entry, CancellationToken cancellationToken)
        {
            var etag = EntityTag.FromXml(null);
            var etagElement = etag.ToXml();
            var key = etagElement.Name;

            IDictionary<XName, XElement> properties;
            if (!_properties.TryGetValue(entry.Path, out properties))
            {
                _properties.Add(entry.Path, new Dictionary<XName, XElement>()
                {
                    [key] = etagElement,
                });
            }
            else
            {
                properties[key] = etagElement;
            }

            return Task.FromResult(etag);
        }

        private IReadOnlyCollection<XElement> GetAll(IEntry entry)
        {
            IReadOnlyCollection<XElement> result;
            IDictionary<XName, XElement> properties;
            if (!_properties.TryGetValue(entry.Path, out properties))
            {
                result = new XElement[0];
            }
            else
            {
                result = properties.Values.ToList();
            }

            return result;
        }

        private void SetAll(IEntry entry, IEnumerable<XElement> elements)
        {
            IDictionary<XName, XElement> properties;
            if (!_properties.TryGetValue(entry.Path, out properties))
                _properties.Add(entry.Path, properties = new Dictionary<XName, XElement>());

            var isEtagEntry = entry is IEntityTagEntry;
            foreach (var element in elements)
            {
                if (isEtagEntry && element.Name == GetETagProperty.PropertyName)
                {
                    _logger.LogWarning("The ETag property must not be set using the property store.");
                    continue;
                }

                properties[element.Name] = element;
            }
        }
    }
}
