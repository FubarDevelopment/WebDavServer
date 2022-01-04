// <copyright file="TextFilePropertyStoreFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Utils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Props.Store.TextFile
{
    /// <summary>
    /// The factory for the <see cref="TextFilePropertyStore"/>.
    /// </summary>
    public class TextFilePropertyStoreFactory : IPropertyStoreFactory
    {
        private readonly IWebDavContextAccessor _webDavContextAccessor;

        private readonly IServiceProvider _serviceProvider;

        private readonly TextFilePropertyStoreOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextFilePropertyStoreFactory"/> class.
        /// </summary>
        /// <param name="options">The options for the text file property store.</param>
        /// <param name="webDavContextAccessor">The WebDAV request context accessor.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public TextFilePropertyStoreFactory(
            IOptions<TextFilePropertyStoreOptions> options,
            IWebDavContextAccessor webDavContextAccessor,
            IServiceProvider serviceProvider)
        {
            _options = options.Value;
            _webDavContextAccessor = webDavContextAccessor;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public IPropertyStore Create(IFileSystem fileSystem)
        {
            var user = _webDavContextAccessor.WebDavContext.User;

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
                    var userName = !user.Identity.IsAnonymous()
                        ? user.Identity?.Name ?? SystemInfo.GetAnonymousUserName()
                        : SystemInfo.GetAnonymousUserName();
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
                var userHomePath = SystemInfo.GetUserHomePath(user);
                rootPath = Path.Combine(userHomePath, ".webdav");
                storeInRoot = true;
            }
            else
            {
                var userName = !user.Identity.IsAnonymous()
                    ? user.Identity?.Name ?? SystemInfo.GetAnonymousUserName()
                    : SystemInfo.GetAnonymousUserName();
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
