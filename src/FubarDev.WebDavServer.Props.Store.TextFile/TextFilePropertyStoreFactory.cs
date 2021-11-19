// <copyright file="TextFilePropertyStoreFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;

using FubarDev.WebDavServer.FileSystem;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Props.Store.TextFile
{
    /// <summary>
    /// The factory for the <see cref="TextFilePropertyStore"/>.
    /// </summary>
    public class TextFilePropertyStoreFactory : IPropertyStoreFactory
    {
        private readonly IWebDavContext _webDavContext;

        private readonly IServiceProvider _serviceProvider;

        private readonly TextFilePropertyStoreOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextFilePropertyStoreFactory"/> class.
        /// </summary>
        /// <param name="options">The options for the text file property store.</param>
        /// <param name="webDavContext">The WebDAV request context.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public TextFilePropertyStoreFactory(
            IOptions<TextFilePropertyStoreOptions> options,
            IWebDavContext webDavContext,
            IServiceProvider serviceProvider)
        {
            _options = options.Value;
            _webDavContext = webDavContext;
            _serviceProvider = serviceProvider;
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

            return ActivatorUtilities.CreateInstance<TextFilePropertyStore>(
                _serviceProvider,
                _options,
                new TextFilePropertyStoreSettings(
                    rootPath,
                    storeInRoot,
                    fileName));
        }
    }
}
