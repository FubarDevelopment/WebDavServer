// <copyright file="DeadPropertyFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props.Store;

namespace FubarDev.WebDavServer.Props.Dead
{
    /// <summary>
    /// A factory for the creation of dead properties.
    /// </summary>
    /// <remarks>
    /// Some dead properties are special (like the <see cref="GetETagProperty"/>), because they have custom implementations.
    /// </remarks>
    public class DeadPropertyFactory : IDeadPropertyFactory
    {
        private readonly IReadOnlyCollection<IDefaultDeadPropertyFactory> _defaultDeadPropertyFactories;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeadPropertyFactory"/> class.
        /// </summary>
        /// <param name="defaultDeadPropertyFactories">Factories for well-known (default) dead properties.</param>
        public DeadPropertyFactory(IEnumerable<IDefaultDeadPropertyFactory> defaultDeadPropertyFactories)
        {
            _defaultDeadPropertyFactories = defaultDeadPropertyFactories.ToList();
        }

        /// <inheritdoc />
        public virtual IDeadProperty Create(IPropertyStore store, IEntry entry, XName name)
        {
            foreach (var deadPropertyFactory in _defaultDeadPropertyFactories)
            {
                if (deadPropertyFactory.TryCreateDeadProperty(store, entry, name, out var deadProp))
                {
                    return deadProp;
                }
            }

            return new DeadProperty(store, entry, name);
        }

        /// <inheritdoc />
        public IDeadProperty Create(IPropertyStore store, IEntry entry, XElement element)
        {
            var result = Create(store, entry, element.Name);
            result.Init(element);
            return result;
        }

        /// <inheritdoc />
        public IEnumerable<IUntypedReadableProperty> GetProperties(IEntry entry)
        {
            return _defaultDeadPropertyFactories
                .SelectMany(factory => factory.GetProperties(entry));
        }
    }
}
