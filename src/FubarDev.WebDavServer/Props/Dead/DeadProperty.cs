// <copyright file="DeadProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props.Store;

namespace FubarDev.WebDavServer.Props.Dead
{
    /// <summary>
    /// The generic dead property
    /// </summary>
    public class DeadProperty : IUntypedWriteableProperty, IDeadProperty
    {
        private readonly IPropertyStore _store;

        private readonly IEntry _entry;

        private XElement _cachedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeadProperty"/> class.
        /// </summary>
        /// <param name="store">The property store for the dead properties</param>
        /// <param name="entry">The file system entry</param>
        /// <param name="name">The XML name of the dead property</param>
        public DeadProperty(IPropertyStore store, IEntry entry, XName name)
        {
            Name = name;
            _store = store;
            _entry = entry;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeadProperty"/> class.
        /// </summary>
        /// <param name="store">The property store for the dead properties</param>
        /// <param name="entry">The file system entry</param>
        /// <param name="element">The element to intialize this property with</param>
        public DeadProperty(IPropertyStore store, IEntry entry, XElement element)
        {
            _store = store;
            _entry = entry;
            Name = element.Name;
        }

        public XName Name { get; }

        public IReadOnlyCollection<XName> AlternativeNames { get; } = new XName[0];

        public int Cost => _store.Cost;

        public Task SetXmlValueAsync(XElement element, CancellationToken ct)
        {
            _cachedValue = element;
            return _store.SetAsync(_entry, element, ct);
        }

        public async Task<XElement> GetXmlValueAsync(CancellationToken ct)
        {
            var result = _cachedValue ?? (_cachedValue = await _store.GetAsync(_entry, Name, ct).ConfigureAwait(false));
            if (result == null)
                throw new InvalidOperationException("Cannot get value from uninitialized property");
            return result;
        }

        public void Init(XElement initialValue)
        {
            _cachedValue = initialValue;
        }
    }
}
