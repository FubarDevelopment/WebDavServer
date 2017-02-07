// <copyright file="InMemoryPropertyStoreFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props.Dead;

namespace FubarDev.WebDavServer.Props.Store.InMemory
{
    public class InMemoryPropertyStoreFactory : IPropertyStoreFactory
    {
        private readonly IDeadPropertyFactory _deadPropertyFactory;

        public InMemoryPropertyStoreFactory(IDeadPropertyFactory deadPropertyFactory = null)
        {
            _deadPropertyFactory = deadPropertyFactory ?? new DeadPropertyFactory();
        }

        public IPropertyStore Create(IFileSystem fileSystem)
        {
            return new InMemoryPropertyStore(_deadPropertyFactory);
        }
    }
}
