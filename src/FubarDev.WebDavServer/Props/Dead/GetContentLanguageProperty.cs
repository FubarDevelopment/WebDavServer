// <copyright file="GetContentLanguageProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Props.Converters;
using FubarDev.WebDavServer.Props.Store;

namespace FubarDev.WebDavServer.Props.Dead
{
    /// <summary>
    /// The implementation of the <c>getcontentlanguage</c> property.
    /// </summary>
    public class GetContentLanguageProperty : SimpleConvertingProperty<string>, IDeadProperty
    {
        /// <summary>
        /// The XML name of the property
        /// </summary>
        public static readonly XName PropertyName = WebDavXml.Dav + "getcontentlanguage";

        private readonly IEntry _entry;

        private readonly IPropertyStore _store;

        private readonly string _defaultContentLanguage;

        private string? _value;

        private bool _isLoaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetContentLanguageProperty"/> class.
        /// </summary>
        /// <param name="entry">The entry to instantiate this property for.</param>
        /// <param name="store">The property store to store this property.</param>
        /// <param name="defaultContentLanguage">The content language to return when none was specified.</param>
        /// <param name="cost">The cost of querying the display names property.</param>
        public GetContentLanguageProperty(IEntry entry, IPropertyStore store, string defaultContentLanguage = "en", int? cost = null)
            : base(PropertyName, null, cost ?? store.Cost, new StringConverter(), WebDavXml.Dav + "contentlanguage")
        {
            _entry = entry;
            _store = store;
            _defaultContentLanguage = defaultContentLanguage;
        }

        /// <summary>
        /// Tries to get the value of this property.
        /// </summary>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A tuple where the first item indicates whether the value was read from the property store and
        /// the second item is the value to be returned as value for this property.</returns>
        public async Task<(bool WasSet, string Value)> TryGetValueAsync(CancellationToken ct)
        {
            var result = await GetValueAsync(ct).ConfigureAwait(false);
            return (_value != null, result);
        }

        /// <inheritdoc />
        public override async Task<string> GetValueAsync(CancellationToken ct)
        {
            if (_value != null || _isLoaded)
            {
                return _value ?? _defaultContentLanguage;
            }

            var storedValue = await _store.GetAsync(_entry, Name, ct).ConfigureAwait(false);
            if (storedValue != null)
            {
                Language = storedValue.Attribute(XNamespace.Xml + "lang")?.Value;
                return _value = storedValue.Value;
            }

            _isLoaded = true;
            return _value ?? _defaultContentLanguage;
        }

        /// <inheritdoc />
        public override async Task SetValueAsync(string value, CancellationToken ct)
        {
            _value = value;
            var element = await GetXmlValueAsync(ct).ConfigureAwait(false);
            await _store.SetAsync(_entry, element, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void Init(XElement initialValue)
        {
            _value = Converter.FromElement(initialValue);
        }

        /// <inheritdoc />
        public bool IsDefaultValue(XElement element)
        {
            return false;
        }
    }
}
