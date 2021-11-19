// <copyright file="InMemoryPropertyStoreFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.WebDavServer.FileSystem;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.WebDavServer.Props.Store.InMemory
{
    /// <summary>
    /// The factory for the <see cref="InMemoryPropertyStore"/>.
    /// </summary>
    public class InMemoryPropertyStoreFactory : IPropertyStoreFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryPropertyStoreFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public InMemoryPropertyStoreFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public IPropertyStore Create(IFileSystem fileSystem)
        {
            return ActivatorUtilities.CreateInstance<InMemoryPropertyStore>(_serviceProvider);
        }
    }
}
