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
using FubarDev.WebDavServer.Properties.Dead;

namespace FubarDev.WebDavServer.Properties.Store.InMemory
{
    public class InMemoryPropertyStore : PropertyStoreBase
    {
        private static readonly IReadOnlyCollection<XElement> _emptyElements = new XElement[0];
        private readonly IDictionary<Uri, IDictionary<XName, XElement>> _properties = new Dictionary<Uri, IDictionary<XName, XElement>>();

        public InMemoryPropertyStore(IDeadPropertyFactory deadPropertyFactory)
            : base(deadPropertyFactory)
        {
        }

        public override int Cost { get; } = 0;

        public override Task<IReadOnlyCollection<XElement>> GetAsync(IEntry entry, CancellationToken cancellationToken)
        {
            IDictionary<XName, XElement> properties;
            if (!_properties.TryGetValue(entry.Path, out properties))
                return Task.FromResult(_emptyElements);

            return Task.FromResult<IReadOnlyCollection<XElement>>(properties.Values.ToList());
        }

        public override Task SetAsync(IEntry entry, IEnumerable<XElement> elements, CancellationToken cancellationToken)
        {
            IDictionary<XName, XElement> properties;
            if (!_properties.TryGetValue(entry.Path, out properties))
                _properties.Add(entry.Path, properties = new Dictionary<XName, XElement>());

            foreach (var element in elements)
            {
                properties[element.Name] = element;
            }

            return Task.FromResult(0);
        }

        public override Task<IReadOnlyCollection<bool>> RemoveAsync(IEntry entry, IEnumerable<XName> names, CancellationToken cancellationToken)
        {
            var result = new List<bool>();
            IDictionary<XName, XElement> properties;
            if (!_properties.TryGetValue(entry.Path, out properties))
            {
                result.AddRange(names.Select(x => false));
            }
            else
            {
                foreach (var name in names)
                {
                    result.Add(properties.Remove(name));
                }

                if (properties.Count == 0)
                    _properties.Remove(entry.Path);
            }

            return Task.FromResult<IReadOnlyCollection<bool>>(result);
        }
    }
}
