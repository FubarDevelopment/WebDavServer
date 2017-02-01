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
    public class MoveHandler : CopyMoveHandlerBase, IMoveHandler
    {
        private readonly MoveHandlerOptions _options;

        public MoveHandler(IFileSystem rootFileSystem, IWebDavHost host, IOptions<MoveHandlerOptions> options, ILogger<MoveHandler> logger, IRemoteHttpClientFactory remoteHttpClientFactory = null)
            : base(rootFileSystem, host, logger, remoteHttpClientFactory)
        {
            _options = options?.Value ?? new MoveHandlerOptions();
        }

        public IEnumerable<string> HttpMethods { get; } = new[] {"MOVE"};

        public Task<IWebDavResult> MoveAsync(string sourcePath, Uri destination, bool? overwrite, CancellationToken cancellationToken)
        {
            var doOverwrite = overwrite ?? _options.OverwriteAsDefault;
            return ExecuteAsync(sourcePath, destination, Depth.Infinity, doOverwrite, _options.Mode, cancellationToken);
        }

        protected override RemoteHttpClientTargetActions CreateRemoteTargetActions(HttpClient httpClient)
        {
            return new MoveRemoteHttpClientTargetActions(httpClient);
        }

        protected override ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> CreateLocalTargetActions(RecursiveProcessingMode mode)
        {
            if (mode == RecursiveProcessingMode.PreferFastest)
                return new MoveInFileSystemTargetAction();
            return new MoveBetweenFileSystemsTargetAction();
        }
    }
}
