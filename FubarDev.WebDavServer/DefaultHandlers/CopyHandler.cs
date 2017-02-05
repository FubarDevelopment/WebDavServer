// <copyright file="CopyHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Engines;
using FubarDev.WebDavServer.Engines.Local;
using FubarDev.WebDavServer.Engines.Remote;
using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.DefaultHandlers
{
    public class CopyHandler : CopyMoveHandlerBase, ICopyHandler
    {
        private readonly CopyHandlerOptions _options;

        public CopyHandler(IFileSystem rootFileSystem, IWebDavHost host, IOptions<CopyHandlerOptions> options, ILogger<CopyHandler> logger, IRemoteHttpClientFactory remoteHttpClientFactory = null)
            : base(rootFileSystem, host, logger, remoteHttpClientFactory)
        {
            _options = options?.Value ?? new CopyHandlerOptions();
        }

        public IEnumerable<string> HttpMethods { get; } = new[] { "COPY" };

        public Task<IWebDavResult> CopyAsync(string sourcePath, Uri destination, Depth depth, bool? overwrite, CancellationToken cancellationToken)
        {
            var doOverwrite = overwrite ?? _options.OverwriteAsDefault;
            return ExecuteAsync(sourcePath, destination, depth, doOverwrite, _options.Mode, cancellationToken);
        }

        protected override RemoteHttpClientTargetActions CreateRemoteTargetActions(HttpClient httpClient)
        {
            return new CopyRemoteHttpClientTargetActions(httpClient);
        }

        protected override ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> CreateLocalTargetActions(RecursiveProcessingMode mode)
        {
            if (mode == RecursiveProcessingMode.PreferFastest)
                return new CopyInFileSystemTargetAction();
            return new CopyBetweenFileSystemsTargetAction();
        }
    }
}
