// <copyright file="Class1DeadPropertyFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

using FubarDev.WebDavServer.Dispatchers;
using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Models;
using FubarDev.WebDavServer.Props.Converters;
using FubarDev.WebDavServer.Props.Live;
using FubarDev.WebDavServer.Props.Store;

using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Props.Dead
{
    /// <summary>
    /// Implementation of <see cref="IDefaultDeadPropertyFactory"/> for WebDAV class 1.
    /// </summary>
    public class Class1DeadPropertyFactory : IDefaultDeadPropertyFactory
    {
        private readonly Dictionary<XName, CreateDeadPropertyDelegate> _defaultCreationMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="Class1DeadPropertyFactory"/> class.
        /// </summary>
        /// <param name="mimeTypeDetector">The mime type detector for the <c>getmimetype</c> property.</param>
        /// <param name="options">The options for the WebDAV class 1 implementation.</param>
        public Class1DeadPropertyFactory(
            IMimeTypeDetector mimeTypeDetector,
            IOptions<WebDavDispatcherClass1Options> options)
        {
            _defaultCreationMap = CreateDeadPropertiesMap(options.Value, mimeTypeDetector);
        }

        private delegate IDeadProperty CreateDeadPropertyDelegate(IPropertyStore store, IEntry entry, XName name);

        /// <inheritdoc />
        public bool TryCreateDeadProperty(IPropertyStore store, IEntry entry, XName name, [NotNullWhen(true)] out IDeadProperty? deadProperty)
        {
            if (!_defaultCreationMap.TryGetValue(name, out var createDeadProp))
            {
                deadProperty = null;
                return false;
            }

            deadProperty = createDeadProp(store, entry, name);
            return true;
        }

        /// <inheritdoc />
        public IEnumerable<IUntypedReadableProperty> GetProperties(IEntry entry)
        {
            yield return entry.GetResourceTypeProperty();

            foreach (var property in entry.GetLiveProperties())
            {
                yield return property;
            }

            var propStore = entry.FileSystem.PropertyStore;
            if (propStore != null)
            {
                yield return Create(propStore, entry, DisplayNameProperty.PropertyName);
            }

            if (entry is IDocument doc)
            {
                yield return new ContentLengthProperty(doc.Length);
                if (propStore != null)
                {
                    yield return Create(propStore, entry, GetContentLanguageProperty.PropertyName);
                    yield return Create(propStore, entry, GetContentTypeProperty.PropertyName);
                }
            }
            else
            {
                Debug.Assert(entry is ICollection, "entry is ICollection");
                yield return new ContentLengthProperty(0L);
                if (propStore != null)
                {
                    var contentType = Create(propStore, entry, GetContentTypeProperty.PropertyName);
                    contentType.Init(new StringConverter().ToElement(GetContentTypeProperty.PropertyName, Utils.MimeTypesMap.FolderContentType));
                    yield return contentType;
                }
            }
        }

        private static Dictionary<XName, CreateDeadPropertyDelegate> CreateDeadPropertiesMap(
            WebDavDispatcherClass1Options options,
            IMimeTypeDetector mimeTypeDetector)
        {
            var result = new Dictionary<XName, CreateDeadPropertyDelegate>()
            {
                [EntityTag.PropertyName] = (store, entry, _) => new GetETagProperty(store, entry),
                [DisplayNameProperty.PropertyName] = (store, entry, _) => new DisplayNameProperty(entry, store, options.HideExtensionForDisplayName),
                [GetContentLanguageProperty.PropertyName] = (store, entry, _) => new GetContentLanguageProperty(entry, store),
                [GetContentTypeProperty.PropertyName] = (store, entry, _) => new GetContentTypeProperty(entry, store, mimeTypeDetector),
            };

            return result;
        }

        private IDeadProperty Create(
            IPropertyStore propertyStore,
            IEntry entry,
            XName name)
        {
            return _defaultCreationMap[name](propertyStore, entry, name);
        }
    }
}
