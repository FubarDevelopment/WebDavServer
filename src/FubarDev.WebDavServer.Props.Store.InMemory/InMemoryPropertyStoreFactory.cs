// <copyright file="InMemoryPropertyStoreFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props.Dead;

using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Props.Store.InMemory
{
    /// <summary>
    /// The factory for the <see cref="InMemoryPropertyStore"/>
    /// </summary>
    public class InMemoryPropertyStoreFactory : IPropertyStoreFactory
    {
        private readonly ILogger<InMemoryPropertyStore> _logger;

        private readonly IDeadPropertyFactory _deadPropertyFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryPropertyStoreFactory"/> class.
        /// </summary>
        /// <param name="logger">The logger for the property store factory</param>
        /// <param name="deadPropertyFactory">The factory for dead properties</param>
        public InMemoryPropertyStoreFactory(ILogger<InMemoryPropertyStore> logger, IDeadPropertyFactory deadPropertyFactory = null)
        {
            _logger = logger;
            _deadPropertyFactory = deadPropertyFactory ?? new DeadPropertyFactory();
        }

        /// <inheritdoc />
        public IPropertyStore Create(IFileSystem fileSystem)
        {
            return new InMemoryPropertyStore(_deadPropertyFactory, _logger);
        }
    }
}
