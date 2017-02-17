// <copyright file="InMemoryPropertyStoreFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props.Dead;

using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Props.Store.InMemory
{
    public class InMemoryPropertyStoreFactory : IPropertyStoreFactory
    {
        private readonly ILogger<InMemoryPropertyStore> _logger;

        private readonly IDeadPropertyFactory _deadPropertyFactory;

        public InMemoryPropertyStoreFactory(ILogger<InMemoryPropertyStore> logger, IDeadPropertyFactory deadPropertyFactory = null)
        {
            _logger = logger;
            _deadPropertyFactory = deadPropertyFactory ?? new DeadPropertyFactory();
        }

        public IPropertyStore Create(IFileSystem fileSystem)
        {
            return new InMemoryPropertyStore(_deadPropertyFactory, _logger);
        }
    }
}
