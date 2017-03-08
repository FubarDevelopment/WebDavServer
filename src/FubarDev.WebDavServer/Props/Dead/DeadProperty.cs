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

using JetBrains.Annotations;

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
        public DeadProperty([NotNull] IPropertyStore store, [NotNull] IEntry entry, [NotNull] XName name)
        {
            Name = name;
            _store = store;
            _entry = entry;
            Language = null;
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
            _cachedValue = element;
            Language = element.Attribute(XNamespace.Xml + "lang")?.Value;
        }

        /// <inheritdoc />
        public XName Name { get; }

        /// <inheritdoc />
        public string Language { get; private set; }

        /// <inheritdoc />
        public IReadOnlyCollection<XName> AlternativeNames { get; } = new XName[0];

        /// <inheritdoc />
        public int Cost => _store.Cost;

        /// <inheritdoc />
        public Task SetXmlValueAsync(XElement element, CancellationToken ct)
        {
            _cachedValue = element;
            return _store.SetAsync(_entry, element, ct);
        }

        /// <inheritdoc />
        public async Task<XElement> GetXmlValueAsync(CancellationToken ct)
        {
            XElement result;
            if (_cachedValue == null)
            {
                result = await _store.GetAsync(_entry, Name, ct).ConfigureAwait(false);
            }
            else
            {
                result = _cachedValue;
            }

            if (result == null)
                throw new InvalidOperationException("Cannot get value from uninitialized property");

            return result;
        }

        /// <inheritdoc />
        public void Init(XElement initialValue)
        {
            var lang = initialValue.Attribute(XNamespace.Xml + "lang")?.Value;
            Language = lang;
            _cachedValue = initialValue;
        }
    }
}
