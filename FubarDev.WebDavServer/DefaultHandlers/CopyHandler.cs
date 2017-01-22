using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties.Dead;

using JetBrains.Annotations;

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

            var targetInfo = TargetInfo.FromSelectionResult(destinationSelectionResult);
            var isSameFileSystem = ReferenceEquals(sourceSelectionResult.TargetFileSystem, destinationSelectionResult.TargetFileSystem);
            if (isSameFileSystem && _options.Mode == RecursiveProcessingMode.PreferFastest)
            {
                // Copy one item inside the same file system (fast)
                return await CopyWithinFileSystem(sourceUrl, destinationUrl, sourceSelectionResult, targetInfo, forbidOverwrite, cancellationToken).ConfigureAwait(false);
            }

            // Copy one item to another file system (probably slow)
            return await CopyBetweenFileSystems(sourceUrl, destinationUrl, sourceSelectionResult, targetInfo, forbidOverwrite, cancellationToken).ConfigureAwait(false);
        }

        private Task<IWebDavResult> CopyBetweenFileSystems(Uri sourceUrl, Uri destinationUrl, SelectionResult sourceSelectionResult, TargetInfo targetInfo, bool forbidOverwrite, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task<IWebDavResult> CopyWithinFileSystem(Uri sourceUrl, Uri destinationUrl, SelectionResult sourceSelectionResult, TargetInfo targetInfo, bool forbidOverwrite, CancellationToken cancellationToken)
        {
            if (sourceSelectionResult.Document != null)
            {
                throw new NotImplementedException();
            }

            Debug.Assert(sourceSelectionResult.Collection != null, "sourceSelectionResult.Collection != null");
            var nodes = await sourceSelectionResult.Collection.GetNodeAsync(Depth.Infinity.OrderValue, cancellationToken).ConfigureAwait(false);
            throw new NotImplementedException();
        }

        private Task<IWebDavResult> CopyServerToServerAsync(SelectionResult sourceSelectionResult, Uri destinationUrl, bool forbidOverwrite, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private interface IEntryHandler
        {
            Task<ICollection> ExecuteAsync(ICollection source, ICollection destinationParent, string destinationName, CancellationToken cancellationToken);
            Task<ICollection> ExecuteAsync(ICollection source, ICollection destination, CancellationToken cancellationToken);
            Task<IDocument> ExecuteAsync(IDocument source, ICollection destinationParent, string destinationName, CancellationToken cancellationToken);
            Task<IDocument> ExecuteAsync(IDocument source, IDocument destination, CancellationToken cancellationToken);
            Task SetPropertiesAsync(IEntry destination, IEnumerable<IDeadProperty> properties, CancellationToken cancellationToken);
        }

        private class TargetInfo
        {
            public TargetInfo([CanBeNull] IEntry parentEntry, [CanBeNull] IEntry targeEntry, [NotNull] string targetName)
            {
                ParentEntry = parentEntry;
                TargetEntry = targeEntry;
                TargetName = targetName;
            }

            [CanBeNull]
            public IEntry ParentEntry { get; }

            [CanBeNull]
            public IEntry TargetEntry { get; }

            [NotNull]
            public string TargetName { get; }

            public static TargetInfo FromSelectionResult(SelectionResult selectionResult)
            {
                if (selectionResult.IsMissing)
                {
                    if (selectionResult.MissingNames.Count != 1)
                        throw new InvalidOperationException();
                    return new TargetInfo(selectionResult.Collection, null, selectionResult.MissingNames.Single());
                }

                if (selectionResult.ResultType == SelectionResultType.FoundCollection)
                {
                    return new TargetInfo(selectionResult.Collection.Parent, selectionResult.Collection, selectionResult.Collection.Name);
                }

                Debug.Assert(selectionResult.Document != null, "selectionResult.Document != null");
                return new TargetInfo(selectionResult.Collection, selectionResult.Document, selectionResult.Document.Name);
            }
        }
    }
}
