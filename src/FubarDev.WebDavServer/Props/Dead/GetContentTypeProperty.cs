// <copyright file="GetContentTypeProperty.cs" company="Fubar Development Junker">
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
    /// <summary>
    /// The implementation of the <c>getcontenttype</c> property
    /// </summary>
    public class GetContentTypeProperty : GenericStringProperty, IDeadProperty
    {
        /// <summary>
        /// The XML name of the property
        /// </summary>
        public static readonly XName PropertyName = WebDavXml.Dav + "getcontenttype";

        private readonly IEntry _entry;

        private readonly IPropertyStore _store;

        private string _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetContentTypeProperty"/> class.
        /// </summary>
        /// <param name="entry">The entry to instantiate this property for</param>
        /// <param name="store">The property store to store this property</param>
        /// <param name="cost">The cost of querying the display names property</param>
        public GetContentTypeProperty(IEntry entry, IPropertyStore store, int? cost = null)
            : base(PropertyName, cost ?? store.Cost, null, null, WebDavXml.Dav + "contenttype")
        {
            _entry = entry;
            _store = store;
        }

        /// <inheritdoc />
        public override async Task<string> GetValueAsync(CancellationToken ct)
        {
            if (_value != null)
                return _value;

            var storedValue = await _store.GetAsync(_entry, Name, ct).ConfigureAwait(false);
            if (storedValue != null)
            {
                return storedValue.Value;
            }

            var fileExt = Path.GetExtension(_entry.Name);
            if (string.IsNullOrEmpty(fileExt))
                return Utils.MimeTypesMap.DefaultMimeType;

            var newName = Utils.MimeTypesMap.GetMimeType(fileExt.Substring(1));
            return newName;
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
