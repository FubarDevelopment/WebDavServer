// <copyright file="TextFilePropertyStoreFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props.Dead;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Props.Store.TextFile
{
    /// <summary>
    /// The factory for the <see cref="TextFilePropertyStore"/>
    /// </summary>
    public class TextFilePropertyStoreFactory : IPropertyStoreFactory
    {
        [NotNull]
        private readonly IDeadPropertyFactory _deadPropertyFactory;

        [NotNull]
        private readonly IWebDavContext _webDavContext;

        [NotNull]
        private readonly TextFilePropertyStoreOptions _options;

        [NotNull]
        private readonly ILogger<TextFilePropertyStore> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextFilePropertyStoreFactory"/> class.
        /// </summary>
        /// <param name="options">The options for the text file property store</param>
        /// <param name="deadPropertyFactory">The factory for the dead properties</param>
        /// <param name="webDavContext">The WebDAV request context</param>
        /// <param name="logger">The logger for the property store factory</param>
        public TextFilePropertyStoreFactory(IOptions<TextFilePropertyStoreOptions> options, [NotNull] IDeadPropertyFactory deadPropertyFactory, [NotNull] IWebDavContext webDavContext, [NotNull] ILogger<TextFilePropertyStore> logger)
        {
            _options = options?.Value ?? new TextFilePropertyStoreOptions();
            _logger = logger;
            _deadPropertyFactory = deadPropertyFactory;
            _webDavContext = webDavContext;
        }

        /// <inheritdoc />
        public IPropertyStore Create(IFileSystem fileSystem)
        {
            if (_options.StoreInTargetFileSystem)
            {
                var localFs = fileSystem as ILocalFileSystem;
                if (localFs != null)
                {
                    return new TextFilePropertyStore(_options, _deadPropertyFactory, localFs.RootDirectoryPath, _logger);
                }
            }

            var userHomePath = Utils.SystemInfo.GetUserHomePath(_webDavContext.User);
            var rootPath = Path.Combine(userHomePath, ".webdav");
            Directory.CreateDirectory(rootPath);

            return new TextFilePropertyStore(_options, _deadPropertyFactory, rootPath, _logger);
        }
    }
}
