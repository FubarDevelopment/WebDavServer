// <copyright file="DotNetFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Properties.Store;

using Microsoft.VisualStudio.Threading;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    public class DotNetFileSystem : ILocalFileSystem
    {
        private readonly PathTraversalEngine _pathTraversalEngine;

        public DotNetFileSystem(DotNetFileSystemOptions options, string rootFolder, PathTraversalEngine pathTraversalEngine, IPropertyStoreFactory propertyStoreFactory = null)
        {
            RootDirectoryPath = rootFolder;
            _pathTraversalEngine = pathTraversalEngine;
            var rootDir = new DotNetDirectory(this, null, new DirectoryInfo(rootFolder), new Uri(string.Empty, UriKind.Relative));
            Root = new AsyncLazy<ICollection>(() => Task.FromResult<ICollection>(rootDir));
            Options = options;
            PropertyStore = propertyStoreFactory?.Create(this);
        }

        public string RootDirectoryPath { get; }

        public AsyncLazy<ICollection> Root { get; }

        public DotNetFileSystemOptions Options { get; }

        public IPropertyStore PropertyStore { get; }

        public Task<SelectionResult> SelectAsync(string path, CancellationToken ct)
        {
            return _pathTraversalEngine.TraverseAsync(this, path, ct);
        }
    }
}
