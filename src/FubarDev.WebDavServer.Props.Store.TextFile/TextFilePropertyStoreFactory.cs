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
    /// <summary>
    /// The factory for the <see cref="TextFilePropertyStore"/>
    /// </summary>
    public class TextFilePropertyStoreFactory : IPropertyStoreFactory
    {
        private readonly IDeadPropertyFactory _deadPropertyFactory;
        private readonly TextFilePropertyStoreOptions _options;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TextFilePropertyStore> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextFilePropertyStoreFactory"/> class.
        /// </summary>
        /// <param name="options">The options for the text file property store</param>
        /// <param name="cache">The in-memory cache for the properties file</param>
        /// <param name="deadPropertyFactory">The factory for the dead properties</param>
        /// <param name="logger">The logger for the property store factory</param>
        public TextFilePropertyStoreFactory(IOptions<TextFilePropertyStoreOptions> options, IMemoryCache cache, IDeadPropertyFactory deadPropertyFactory, ILogger<TextFilePropertyStore> logger)
        {
            _options = options?.Value ?? new TextFilePropertyStoreOptions();
            _cache = cache;
            _logger = logger;
            _deadPropertyFactory = deadPropertyFactory;
        }

        /// <inheritdoc />
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
