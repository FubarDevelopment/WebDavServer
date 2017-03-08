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

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props.Dead
{
    /// <summary>
    /// The <c>displayname</c> property
    /// </summary>
    public class DisplayNameProperty : GenericStringProperty, IDeadProperty
    {
        /// <summary>
        /// The XML name of the property
        /// </summary>
        public static readonly XName PropertyName = WebDavXml.Dav + "displayname";

        private readonly IEntry _entry;

        private readonly IPropertyStore _store;

        private readonly bool _hideExtension;

        private string _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayNameProperty"/> class.
        /// </summary>
        /// <param name="entry">The entry to instantiate this property for</param>
        /// <param name="store">The property store to store this property</param>
        /// <param name="hideExtension">Hide the extension from the display name</param>
        /// <param name="cost">The cost of querying the display names property</param>
        public DisplayNameProperty([NotNull] IEntry entry, [NotNull] IPropertyStore store, bool hideExtension, int? cost = null)
            : base(PropertyName, null, cost ?? store.Cost, null, null)
        {
            _entry = entry;
            _store = store;
            _hideExtension = hideExtension;
        }

        /// <inheritdoc />
        public override async Task<string> GetValueAsync(CancellationToken ct)
        {
            if (_value != null)
                return _value;

            if (_store != null)
            {
                var storedValue = await _store.GetAsync(_entry, Name, ct).ConfigureAwait(false);
                if (storedValue != null)
                {
                    Language = storedValue.Attribute(XNamespace.Xml + "lang")?.Value;
                    return _value = storedValue.Value;
                }
            }

            var newName = _value = _hideExtension ? Path.GetFileNameWithoutExtension(_entry.Name) : _entry.Name;
            return newName;
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
    }
}
