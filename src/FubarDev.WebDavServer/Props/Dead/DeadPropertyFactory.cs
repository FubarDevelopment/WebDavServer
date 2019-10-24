// <copyright file="DeadPropertyFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props.Store;

using Microsoft.Extensions.DependencyInjection;

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
        private readonly Lazy<IWebDavDispatcher> _webDavDispatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeadPropertyFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to query the <see cref="Dispatchers.IWebDavClass"/> implementations.</param>
        public DeadPropertyFactory(IServiceProvider serviceProvider)
        {
            _webDavDispatcher = new Lazy<IWebDavDispatcher>(serviceProvider.GetRequiredService<IWebDavDispatcher>);
        }

        /// <inheritdoc />
        public virtual IDeadProperty Create(IPropertyStore store, IEntry entry, XName name)
        {
            foreach (var webDavClass in _webDavDispatcher.Value.SupportedClasses)
            {
                if (webDavClass.TryCreateDeadProperty(store, entry, name, out var deadProp))
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
    }
}
