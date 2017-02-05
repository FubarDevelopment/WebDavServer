// <copyright file="TextFilePropertyStoreFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Properties.Dead;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Properties.Store.TextFile
{
    public class TextFilePropertyStoreFactory : IPropertyStoreFactory
    {
        private readonly IDeadPropertyFactory _deadPropertyFactory;
        private readonly TextFilePropertyStoreOptions _options;
        private readonly IMemoryCache _cache;

        public TextFilePropertyStoreFactory(IOptions<TextFilePropertyStoreOptions> options, IMemoryCache cache, IDeadPropertyFactory deadPropertyFactory)
            : this(options.Value, cache)
        {
            _deadPropertyFactory = deadPropertyFactory;
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
                    return new TextFilePropertyStore(_options, _cache, _deadPropertyFactory, localFs.RootDirectoryPath);
                }
            }

            return new TextFilePropertyStore(_options, _cache, _deadPropertyFactory);
        }
    }
}
