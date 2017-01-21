using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;

using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.DefaultHandlers
{
    public class CopyHandler : ICopyHandler
    {
        private readonly IFileSystem _rootFileSystem;
        private readonly IWebDavHost _host;
        private readonly CopyHandlerOptions _options;

        public CopyHandler(IFileSystem rootFileSystem, IWebDavHost host, IOptions<CopyHandlerOptions> options)
        {
            _rootFileSystem = rootFileSystem;
            _host = host;
            _options = options?.Value ?? new CopyHandlerOptions();
        }

        public IEnumerable<string> HttpMethods { get; } = new[] {"COPY"};

        public async Task<IWebDavResult> CopyAsync(string sourcePath, Uri destination, bool forbidOverwrite, CancellationToken cancellationToken)
        {
            var sourceSelectionResult = await _rootFileSystem.SelectAsync(sourcePath, cancellationToken).ConfigureAwait(false);
            if (sourceSelectionResult.IsMissing)
                throw new WebDavException(WebDavStatusCodes.NotFound);

            var sourceUrl = new Uri(_host.BaseUrl, sourcePath);
            var destinationUrl = new Uri(sourceUrl, destination);
            if (!_host.BaseUrl.IsBaseOf(destinationUrl) || _options.Mode == RecursiveProcessingMode.PreferCrossServer)
            {
                // Copy from server to server (slow)
                return await CopyServerToServerAsync(sourceSelectionResult, destinationUrl, forbidOverwrite, cancellationToken).ConfigureAwait(false);
            }

            // Copy from one known file system to another
            var destinationPath = _host.BaseUrl.MakeRelativeUri(destinationUrl).ToString();
            var destinationSelectionResult = await _rootFileSystem.SelectAsync(destinationPath, cancellationToken).ConfigureAwait(false);
            if (destinationSelectionResult.IsMissing && destinationSelectionResult.MissingNames.Count != 1)
                throw new WebDavException(WebDavStatusCodes.NotFound);

            Debug.Assert(sourceSelectionResult.TargetEntry != null, "sourceSelectionResult.TargetEntry != null");
            Debug.Assert(destinationSelectionResult.TargetEntry != null, "destinationSelectionResult.TargetEntry != null");
            var isSameFileSystem = ReferenceEquals(sourceSelectionResult.TargetEntry.FileSystem, destinationSelectionResult.TargetEntry.FileSystem);
            if (isSameFileSystem && _options.Mode == RecursiveProcessingMode.PreferFastest)
            {
                // Copy one item inside the same file system (fast)
                return await CopyWithinFileSystem(sourceUrl, destinationUrl, sourceSelectionResult, destinationSelectionResult, forbidOverwrite, cancellationToken).ConfigureAwait(false);
            }

            // Copy one item to another file system (probably slow)
            return await CopyBetweenFileSystems(sourceUrl, destinationUrl, sourceSelectionResult, destinationSelectionResult, forbidOverwrite, cancellationToken).ConfigureAwait(false);
        }

        private Task<IWebDavResult> CopyBetweenFileSystems(Uri sourceUrl, Uri destinationUrl, SelectionResult sourceSelectionResult, SelectionResult destinationSelectionResult, bool forbidOverwrite, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private Task<IWebDavResult> CopyWithinFileSystem(Uri sourceUrl, Uri destinationUrl, SelectionResult sourceSelectionResult, SelectionResult destinationSelectionResult, bool forbidOverwrite, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private Task<IWebDavResult> CopyServerToServerAsync(SelectionResult sourceSelectionResult, Uri destinationUrl, bool forbidOverwrite, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
