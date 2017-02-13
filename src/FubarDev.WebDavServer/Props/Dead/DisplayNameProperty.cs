// <copyright file="DisplayNameProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Props.Generic;
using FubarDev.WebDavServer.Props.Store;

namespace FubarDev.WebDavServer.Props.Dead
{
    public class DisplayNameProperty : GenericStringProperty, IDeadProperty
    {
        public static readonly XName PropertyName = WebDavXml.Dav + "displayname";

        private readonly IEntry _entry;

        private readonly IPropertyStore _store;

        private readonly bool _hideExtension;

        private string _value;

        public DisplayNameProperty(IEntry entry, IPropertyStore store, bool hideExtension, int? cost = null)
            : base(PropertyName, cost ?? store.Cost, null, null)
        {
            _entry = entry;
            _store = store;
            _hideExtension = hideExtension;
        }

        public override async Task<string> GetValueAsync(CancellationToken ct)
        {
            if (_value != null)
                return _value;

            if (_store != null)
            {
                var storedValue = await _store.GetAsync(_entry, Name, ct).ConfigureAwait(false);
                if (storedValue != null)
                {
                    return storedValue.Value;
                }
            }

            var newName = _value = _hideExtension ? Path.GetFileNameWithoutExtension(_entry.Name) : _entry.Name;
            return newName;
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
