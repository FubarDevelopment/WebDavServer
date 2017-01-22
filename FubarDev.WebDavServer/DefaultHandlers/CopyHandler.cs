using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties;
using FubarDev.WebDavServer.Properties.Dead;
using FubarDev.WebDavServer.Properties.Live;

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
                throw new WebDavException(WebDavStatusCodes.Conflict);

            var targetInfo = TargetInfo.FromSelectionResult(destinationSelectionResult, destinationUrl);
            var isSameFileSystem = ReferenceEquals(sourceSelectionResult.TargetFileSystem, destinationSelectionResult.TargetFileSystem);
            if (isSameFileSystem && _options.Mode == RecursiveProcessingMode.PreferFastest)
            {
                // Copy one item inside the same file system (fast)
                return await CopyWithinFileSystemAsync(sourceUrl, destinationUrl, sourceSelectionResult, targetInfo, forbidOverwrite, cancellationToken).ConfigureAwait(false);
            }

            // Copy one item to another file system (probably slow)
            return await CopyBetweenFileSystemsAsync(sourceUrl, destinationUrl, sourceSelectionResult, targetInfo, forbidOverwrite, cancellationToken).ConfigureAwait(false);
        }

        private Task<IWebDavResult> CopyBetweenFileSystemsAsync(Uri sourceUrl, Uri destinationUrl, SelectionResult sourceSelectionResult, TargetInfo targetInfo, bool forbidOverwrite, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task<IWebDavResult> CopyWithinFileSystemAsync(Uri sourceUrl, Uri destinationUrl, SelectionResult sourceSelectionResult, TargetInfo targetInfo, bool forbidOverwrite, CancellationToken cancellationToken)
        {
            if (sourceSelectionResult.Document != null)
            {
                throw new NotImplementedException();
            }

            Debug.Assert(sourceSelectionResult.Collection != null, "sourceSelectionResult.Collection != null");
            var nodes = await sourceSelectionResult.Collection.GetNodeAsync(Depth.Infinity.OrderValue, cancellationToken).ConfigureAwait(false);
            var processor = new NodeProcessor(new CopyWithinFileSystemHandler(), _host);
            var errors = await processor.ExecuteAsync(nodes, targetInfo, cancellationToken).ConfigureAwait(false);

            if (errors.Count == 0)
            {
                if (targetInfo.TargetEntry == null)
                    return new WebDavResult(WebDavStatusCodes.Created);
                return new WebDavResult(WebDavStatusCodes.NoContent);
            }

            return new WebDavResult<Multistatus>(
                WebDavStatusCodes.MultiStatus, 
                new Multistatus()
                {
                    Response = errors.ToList()
                });
        }

        private Task<IWebDavResult> CopyServerToServerAsync(SelectionResult sourceSelectionResult, Uri destinationUrl, bool forbidOverwrite, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private class NodeProcessor
        {
            private readonly IEntryHandler _handler;
            private readonly IWebDavHost _host;

            public NodeProcessor(IEntryHandler handler, IWebDavHost host)
            {
                _handler = handler;
                _host = host;
            }

            public async Task<IImmutableList<Response>> ExecuteAsync(CollectionExtensions.INode node, TargetInfo targetInfo, CancellationToken cancellationToken)
            {
                ICollection targetCollection;
                if (targetInfo.TargetEntry == null)
                {
                    Debug.Assert(targetInfo.ParentEntry != null, "targetInfo.ParentEntry != null");
                    targetCollection = await targetInfo.ParentEntry.CreateCollectionAsync(targetInfo.TargetName, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    targetCollection = targetInfo.TargetEntry as ICollection;
                }

                IImmutableList<Response> result;
                if (targetCollection == null)
                {
                    var status = WebDavStatusCodes.Conflict;
                    result = ImmutableList.Create<Response>().Add(new Response()
                    {
                        Href = targetInfo.DestinationUri.ToString(),
                        ItemsElementName = new List<ItemsChoiceType2>() {ItemsChoiceType2.Status},
                        Items = new List<object>() {$"{_host.RequestProtocol} {(int) status} {status.GetReasonPhrase()}"},
                    });
                }
                else
                {
                    result = await ExecuteAsync(node, targetCollection, cancellationToken).ConfigureAwait(false);
                }

                return result;
            }

            private async Task<IImmutableList<Response>> ExecuteAsync(CollectionExtensions.INode node, ICollection target,
                                                                      CancellationToken cancellationToken)
            {
                var properties = await node.Collection.GetProperties().ToList(cancellationToken).ConfigureAwait(false);
                await SetPropertiesAsync(target, properties, cancellationToken).ConfigureAwait(false);
                throw new NotImplementedException();
            }

            private async Task SetPropertiesAsync(IEntry destination, IEnumerable<IUntypedReadableProperty> properties, CancellationToken cancellationToken)
            {
                var liveProperties = new List<ILiveProperty>();
                var deadProperties = new List<IDeadProperty>();
                foreach (var property in properties)
                {
                    var liveProp = property as ILiveProperty;
                    if (liveProp != null)
                    {
                        liveProperties.Add(liveProp);
                    }
                    else
                    {
                        var deadProp = (IDeadProperty) property;
                        deadProperties.Add(deadProp);
                    }
                }

                if (liveProperties.Count != 0)
                    await SetPropertiesAsync(destination, liveProperties, cancellationToken).ConfigureAwait(false);

                if (deadProperties.Count != 0)
                    await SetPropertiesAsync(destination, deadProperties, cancellationToken).ConfigureAwait(false);
            }

            private async Task SetPropertiesAsync(IEntry destination, IEnumerable<IDeadProperty> properties, CancellationToken cancellationToken)
            {
                var propertyStore = destination.FileSystem.PropertyStore;
                if (propertyStore == null)
                    return;

                var elements = new List<XElement>();
                foreach (var property in properties)
                {
                    elements.Add(await property.GetXmlValueAsync(cancellationToken).ConfigureAwait(false));
                }

                await propertyStore.SetAsync(destination, elements, cancellationToken).ConfigureAwait(false);
            }

            private async Task SetPropertiesAsync(IEntry destination, IEnumerable<ILiveProperty> properties, CancellationToken cancellationToken)
            {
                var propNameToValue = new Dictionary<XName, XElement>();
                foreach (var property in properties)
                {
                    propNameToValue[property.Name] = await property.GetXmlValueAsync(cancellationToken).ConfigureAwait(false);
                }

                using (var propEnum = destination.GetProperties().GetEnumerator())
                {
                    while (await propEnum.MoveNext(cancellationToken).ConfigureAwait(false))
                    {
                        var prop = propEnum.Current as IUntypedWriteableProperty;
                        XElement propValue;
                        if (prop != null && propNameToValue.TryGetValue(prop.Name, out propValue))
                        {
                            await prop.SetXmlValueAsync(propValue, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        private class CopyFromFileSyswtemToFileSystemHandler : IEntryHandler
        {
            public async Task<IDocument> ExecuteAsync(IDocument source, ICollection destinationParent, string destinationName, CancellationToken cancellationToken)
            {
                var doc = await destinationParent.CreateDocumentAsync(destinationName, cancellationToken).ConfigureAwait(false);
                return await ExecuteAsync(source, doc, cancellationToken).ConfigureAwait(false);
            }

            public async Task<IDocument> ExecuteAsync(IDocument source, IDocument destination, CancellationToken cancellationToken)
            {
                using (var sourceStream = await source.OpenReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    using (var destinationStream = await destination.CreateAsync(cancellationToken).ConfigureAwait(false))
                    {
                        await sourceStream.CopyToAsync(destinationStream, 65536, cancellationToken).ConfigureAwait(false);
                    }
                }

                return destination;
            }

            public Task ExecuteAsync(ICollection source, CancellationToken cancellationToken)
            {
                return Task.FromResult(0);
            }
        }

        private class CopyWithinFileSystemHandler : IEntryHandler
        {
            public Task<IDocument> ExecuteAsync(IDocument source, ICollection destinationParent, string destinationName, CancellationToken cancellationToken)
            {
                return source.CopyToAsync(destinationParent, destinationName, cancellationToken);
            }

            public Task<IDocument> ExecuteAsync(IDocument source, IDocument destination, CancellationToken cancellationToken)
            {
                Debug.Assert(destination.Parent != null, "destination.Parent != null");
                return source.CopyToAsync(destination.Parent, destination.Name, cancellationToken);
            }

            public Task ExecuteAsync(ICollection source, CancellationToken cancellationToken)
            {
                return Task.FromResult(0);
            }
        }

        private interface IEntryHandler
        {
            [NotNull, ItemNotNull]
            Task<IDocument> ExecuteAsync([NotNull] IDocument source, [NotNull] ICollection destinationParent, [NotNull] string destinationName, CancellationToken cancellationToken);

            [NotNull, ItemNotNull]
            Task<IDocument> ExecuteAsync([NotNull] IDocument source, [NotNull] IDocument destination, CancellationToken cancellationToken);

            [NotNull]
            Task ExecuteAsync([NotNull] ICollection source, CancellationToken cancellationToken);
        }

        private struct TargetInfo
        {
            public TargetInfo([CanBeNull] ICollection parentEntry, [CanBeNull] IEntry targeEntry, [NotNull] string targetName, [NotNull] Uri destinationUri)
            {
                ParentEntry = parentEntry;
                TargetEntry = targeEntry;
                TargetName = targetName;
                DestinationUri = destinationUri;
            }

            [CanBeNull]
            public ICollection ParentEntry { get; }

            [CanBeNull]
            public IEntry TargetEntry { get; }

            [NotNull]
            public string TargetName { get; }

            [NotNull]
            public Uri DestinationUri { get; }

            public static TargetInfo FromSelectionResult(SelectionResult selectionResult, Uri destinationUri)
            {
                if (selectionResult.IsMissing)
                {
                    if (selectionResult.MissingNames.Count != 1)
                        throw new InvalidOperationException();
                    return new TargetInfo(selectionResult.Collection, null, selectionResult.MissingNames.Single(), destinationUri);
                }

                if (selectionResult.ResultType == SelectionResultType.FoundCollection)
                {
                    return new TargetInfo(selectionResult.Collection.Parent, selectionResult.Collection, selectionResult.Collection.Name, destinationUri);
                }

                Debug.Assert(selectionResult.Document != null, "selectionResult.Document != null");
                return new TargetInfo(selectionResult.Collection, selectionResult.Document, selectionResult.Document.Name, destinationUri);
            }
        }
    }
}
