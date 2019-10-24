// <copyright file="TextFilePropertyStoreFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props.Dead;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Props.Store.TextFile
{
    /// <summary>
    /// The factory for the <see cref="TextFilePropertyStore"/>.
    /// </summary>
    public class TextFilePropertyStoreFactory : IPropertyStoreFactory
    {
        private readonly IDeadPropertyFactory _deadPropertyFactory;

        private readonly IWebDavContext _webDavContext;

        private readonly TextFilePropertyStoreOptions _options;

        private readonly ILogger<TextFilePropertyStore> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextFilePropertyStoreFactory"/> class.
        /// </summary>
        /// <param name="options">The options for the text file property store.</param>
        /// <param name="deadPropertyFactory">The factory for the dead properties.</param>
        /// <param name="webDavContext">The WebDAV request context.</param>
        /// <param name="logger">The logger for the property store factory.</param>
        public TextFilePropertyStoreFactory(IOptions<TextFilePropertyStoreOptions> options, IDeadPropertyFactory deadPropertyFactory, IWebDavContext webDavContext, ILogger<TextFilePropertyStore> logger)
        {
            _options = options?.Value ?? new TextFilePropertyStoreOptions();
            _logger = logger;
            _deadPropertyFactory = deadPropertyFactory;
            _webDavContext = webDavContext;
        }

        /// <inheritdoc />
        public IPropertyStore Create(IFileSystem fileSystem)
        {
            string fileName = ".properties";
            bool storeInRoot;
            string rootPath;
            ILocalFileSystem? localFs;
            if (_options.StoreInTargetFileSystem && (localFs = fileSystem as ILocalFileSystem) != null)
            {
                rootPath = localFs.RootDirectoryPath;
                storeInRoot = !localFs.HasSubfolders;
                if (storeInRoot)
                {
                    var userName = _webDavContext.User.Identity.IsAuthenticated
                        ? _webDavContext.User.Identity.Name
                        : "anonymous";
                    var p = userName.IndexOf('\\');
                    if (p != -1)
                    {
                        userName = userName.Substring(p + 1);
                    }

                    fileName = $"{userName}.json";
                }
            }
            else if (string.IsNullOrEmpty(_options.RootFolder))
            {
                var userHomePath = Utils.SystemInfo.GetUserHomePath(_webDavContext.User);
                rootPath = Path.Combine(userHomePath, ".webdav");
                storeInRoot = true;
            }
            else
            {
                var userName = _webDavContext.User.Identity.IsAuthenticated
                                   ? _webDavContext.User.Identity.Name
                                   : "anonymous";
                rootPath = Path.Combine(_options.RootFolder, userName);
                storeInRoot = true;
            }

            Directory.CreateDirectory(rootPath);

            return new TextFilePropertyStore(_options, _deadPropertyFactory, rootPath, storeInRoot, fileName, _logger);
        }
    }
}
