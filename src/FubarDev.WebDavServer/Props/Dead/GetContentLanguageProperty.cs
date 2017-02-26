// <copyright file="GetContentLanguageProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Props.Generic;
using FubarDev.WebDavServer.Props.Store;

namespace FubarDev.WebDavServer.Props.Dead
{
    /// <summary>
    /// The implementation of the <code>getcontentlanguage</code> property
    /// </summary>
    public class GetContentLanguageProperty : GenericStringProperty, IDeadProperty
    {
        /// <summary>
        /// The XML name of the property
        /// </summary>
        public static readonly XName PropertyName = WebDavXml.Dav + "getcontentlanguage";

        private readonly IEntry _entry;

        private readonly IPropertyStore _store;

        private string _value;

        private bool _isLoaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetContentLanguageProperty"/> class.
        /// </summary>
        /// <param name="entry">The entry to instantiate this property for</param>
        /// <param name="store">The property store to store this property</param>
        /// <param name="cost">The cost of querying the display names property</param>
        public GetContentLanguageProperty(IEntry entry, IPropertyStore store, int? cost = null)
            : base(PropertyName, cost ?? store.Cost, null, null, WebDavXml.Dav + "contentlanguage")
        {
            _entry = entry;
            _store = store;
        }

        /// <summary>
        /// Tries to get the value of this property
        /// </summary>
        /// <param name="ct">The cancellation token</param>
        /// <returns>A tuple where the first item indicates whether the value was read from the property store and
        /// the second item is the value to be returned as value for this property</returns>
        public async Task<ValueTuple<bool, string>> TryGetValueAsync(CancellationToken ct)
        {
            var result = await GetValueAsync(ct).ConfigureAwait(false);
            return ValueTuple.Create(_value != null, result);
        }

        /// <inheritdoc />
        public override async Task<string> GetValueAsync(CancellationToken ct)
        {
            if (_value != null || _isLoaded)
                return _value ?? "en";

            _value = (await _store.GetAsync(_entry, Name, ct).ConfigureAwait(false))?.Value;
            _isLoaded = true;
            return _value ?? "en";
        }

        /// <inheritdoc />
        public override Task SetValueAsync(string value, CancellationToken ct)
        {
            _value = value;
            return _store.SetAsync(_entry, Converter.ToElement(Name, value), ct);
        }

        /// <inheritdoc />
        public void Init(XElement initialValue)
        {
            _value = Converter.FromElement(initialValue);
        }
    }
}
