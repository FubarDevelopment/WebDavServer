using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Properties.Dead;

namespace FubarDev.WebDavServer.Properties.Store.InMemory
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
