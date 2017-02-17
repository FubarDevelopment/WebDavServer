// <copyright file="TextFilePropertyStoreFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props.Dead;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Props.Store.TextFile
{
    public class TextFilePropertyStoreFactory : IPropertyStoreFactory
    {
        private readonly IDeadPropertyFactory _deadPropertyFactory;
        private readonly TextFilePropertyStoreOptions _options;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TextFilePropertyStore> _logger;

        public TextFilePropertyStoreFactory(IOptions<TextFilePropertyStoreOptions> options, IMemoryCache cache, IDeadPropertyFactory deadPropertyFactory, ILogger<TextFilePropertyStore> logger)
            : this(options.Value, cache, logger)
        {
            _deadPropertyFactory = deadPropertyFactory;
        }

        public TextFilePropertyStoreFactory(TextFilePropertyStoreOptions options, IMemoryCache cache, ILogger<TextFilePropertyStore> logger)
        {
            _options = options;
            _cache = cache;
            _logger = logger;
        }

        public IPropertyStore Create(IFileSystem fileSystem)
        {
            if (_options.StoreInTargetFileSystem)
            {
                var localFs = fileSystem as ILocalFileSystem;
                if (localFs != null)
                {
                    return new TextFilePropertyStore(_options, _cache, _deadPropertyFactory, localFs.RootDirectoryPath, _logger);
                }
            }

            return new TextFilePropertyStore(_options, _cache, _deadPropertyFactory, _logger);
        }
    }
}
