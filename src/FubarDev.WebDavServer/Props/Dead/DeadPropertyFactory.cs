// <copyright file="DeadPropertyFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.Props.Store;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Props.Dead
{
    /// <summary>
    /// A factory for the creation of dead properties
    /// </summary>
    /// <remarks>
    /// Some dead properties are special (like the <see cref="GetETagProperty"/>), because they have custom implementations.
    /// </remarks>
    public class DeadPropertyFactory : IDeadPropertyFactory
    {
        private readonly IReadOnlyDictionary<XName, CreateDeadPropertyDelegate> _defaultCreationMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeadPropertyFactory"/> class.
        /// </summary>
        /// <param name="options">The options for the dead property store</param>
        public DeadPropertyFactory(IOptions<DeadPropertyFactoryOptions> options = null)
            : this(options?.Value ?? new DeadPropertyFactoryOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeadPropertyFactory"/> class.
        /// </summary>
        /// <param name="options">The options for the dead property store</param>
        public DeadPropertyFactory([NotNull] DeadPropertyFactoryOptions options)
        {
            _defaultCreationMap = new Dictionary<XName, CreateDeadPropertyDelegate>()
            {
                [EntityTag.PropertyName] = (store, entry, name) => new GetETagProperty(store, entry),
                [DisplayNameProperty.PropertyName] = (store, entry, name) => new DisplayNameProperty(entry, store, options.HideExtensionForDisplayName),
                [GetContentLanguageProperty.PropertyName] = (store, entry, name) => new GetContentLanguageProperty(entry, store),
                [GetContentTypeProperty.PropertyName] = (store, entry, name) => new GetContentTypeProperty(entry, store),
            };
        }

        private delegate IDeadProperty CreateDeadPropertyDelegate(IPropertyStore store, IEntry entry, XName name);

        /// <inheritdoc />
        public virtual IDeadProperty Create(IPropertyStore store, IEntry entry, XName name)
        {
            CreateDeadPropertyDelegate createFunc;
            if (_defaultCreationMap.TryGetValue(name, out createFunc))
                return createFunc(store, entry, name);
            return new DeadProperty(store, entry, name);
        }

        /// <inheritdoc />
        public IDeadProperty Create(IPropertyStore store, IEntry entry, XElement element)
        {
            var result = Create(store, entry, element.Name);
            result.Init(element);
            return result;
        }
    }
}
