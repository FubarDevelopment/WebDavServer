// <copyright file="GetETagProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Models;
using FubarDev.WebDavServer.Props.Converters;
using FubarDev.WebDavServer.Props.Store;

namespace FubarDev.WebDavServer.Props.Dead
{
    /// <summary>
    /// The implementation of the <c>getetag</c> property.
    /// </summary>
    public class GetETagProperty : ITypedReadableProperty<EntityTag>, IDeadProperty
    {
        /// <summary>
        /// The XML name of the property.
        /// </summary>
        public static readonly XName PropertyName = EntityTag.PropertyName;

        private readonly IPropertyStore? _propertyStore;

        private readonly IEntry _entry;

        private readonly IEntityTagEntry? _etagEntry;

        private XElement? _element;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetETagProperty"/> class.
        /// </summary>
        /// <param name="propertyStore">The property store to store this property.</param>
        /// <param name="entry">The entry to instantiate this property for.</param>
        /// <param name="cost">The cost of querying the display names property.</param>
        public GetETagProperty(IPropertyStore? propertyStore, IEntry entry, int? cost = null)
        {
            _propertyStore = propertyStore;
            _entry = entry;
            _etagEntry = entry as IEntityTagEntry;
            Name = PropertyName;
            Cost = cost ?? (_etagEntry != null ? 0 : (int?)null) ?? _propertyStore?.Cost ?? 0;
        }

        /// <inheritdoc />
        public XName Name { get; }

        /// <inheritdoc />
        public string? Language { get; } = null;

        /// <inheritdoc />
        public IReadOnlyCollection<XName> AlternativeNames { get; } = new[] { WebDavXml.Dav + "etag" };

        /// <inheritdoc />
        public int Cost { get; }

        /// <summary>
        /// Gets the entity tag converter.
        /// </summary>
        public IPropertyConverter<EntityTag> Converter { get; } = new EntityTagConverter();

        /// <inheritdoc />
        public async Task<XElement> GetXmlValueAsync(CancellationToken ct)
        {
            if (_etagEntry != null)
            {
                return Converter.ToElement(Name, _etagEntry.ETag);
            }

            if (_element == null)
            {
                if (_propertyStore != null)
                {
                    var etag = await _propertyStore.GetETagAsync(_entry, ct).ConfigureAwait(false);
                    _element = Converter.ToElement(Name, etag);
                }
                else
                {
                    _element = new EntityTag(false).ToXml();
                }
            }

            return _element;
        }

        /// <inheritdoc />
        public void Init(XElement initialValue)
        {
            _element = initialValue;
        }

        /// <inheritdoc />
        public async Task<EntityTag> GetValueAsync(CancellationToken ct)
        {
            return Converter.FromElement(await GetXmlValueAsync(ct).ConfigureAwait(false));
        }

        /// <inheritdoc />
        public bool IsDefaultValue(XElement element)
        {
            return false;
        }
    }
}
