using FubarDev.WebDavServer.FileSystem;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Properties.Store.TextFile
{
    public class TextFilePropertyStoreFactory : IPropertyStoreFactory
    {
        private readonly TextFilePropertyStoreOptions _options;
        private readonly IMemoryCache _cache;

        public TextFilePropertyStoreFactory(IOptions<TextFilePropertyStoreOptions> options, IMemoryCache cache)
            : this(options.Value, cache)
        {
        }

        public TextFilePropertyStoreFactory(TextFilePropertyStoreOptions options, IMemoryCache cache)
        {
            _options = options;
            _cache = cache;
        }

        public IPropertyStore Create(IFileSystem fileSystem)
        {
            if (_options.StoreInTargetFileSystem)
            {
                var localFs = fileSystem as ILocalFileSystem;
                if (localFs != null)
                {
                    return new TextFilePropertyStore(_options, _cache, localFs.RootDirectoryPath);
                }
            }

            return new TextFilePropertyStore(_options, _cache);
        }
    }
}
