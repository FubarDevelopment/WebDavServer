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
    public class GetContentLanguageProperty : GenericStringProperty, IDeadProperty
    {
        public static readonly XName PropertyName = WebDavXml.Dav + "getcontentlanguage";

        private readonly IEntry _entry;

        private readonly IPropertyStore _store;

        private string _value;

        private bool _isLoaded;

        public GetContentLanguageProperty(IEntry entry, IPropertyStore store, int? cost = null)
            : base(PropertyName, cost ?? store.Cost, null, null, WebDavXml.Dav + "contentlanguage")
        {
            _entry = entry;
            _store = store;
        }

        public async Task<ValueTuple<bool, string>> TryGetValueAsync(CancellationToken ct)
        {
            var result = await GetValueAsync(ct).ConfigureAwait(false);
            if (_value == null)
                return ValueTuple.Create(false, _value);
            return ValueTuple.Create(true, result);
        }

        public override async Task<string> GetValueAsync(CancellationToken ct)
        {
            if (_value != null || _isLoaded)
                return _value ?? "en";

            _value = (await _store.GetAsync(_entry, Name, ct).ConfigureAwait(false))?.Value;
            _isLoaded = true;
            return _value ?? "en";
        }

        public override Task SetValueAsync(string value, CancellationToken ct)
        {
            _value = value;
            return _store.SetAsync(_entry, Converter.ToElement(Name, value), ct);
        }

        public void Init(XElement initialValue)
        {
            _value = Converter.FromElement(initialValue);
        }
    }
}
