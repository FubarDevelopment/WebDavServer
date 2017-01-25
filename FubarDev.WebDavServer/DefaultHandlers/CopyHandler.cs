using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Engines;
using FubarDev.WebDavServer.Engines.DefaultTargetAction;
using FubarDev.WebDavServer.Engines.FileSystemTargets;
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

        public async Task<IWebDavResult> CopyAsync(string sourcePath, Uri destination, bool? overwrite, CancellationToken cancellationToken)
        {
            var sourceSelectionResult = await _rootFileSystem.SelectAsync(sourcePath, cancellationToken).ConfigureAwait(false);
            if (sourceSelectionResult.IsMissing)
                throw new WebDavException(WebDavStatusCodes.NotFound);

            var sourceUrl = new Uri(_host.BaseUrl, sourcePath);
            var destinationUrl = new Uri(sourceUrl, destination);
            if (!_host.BaseUrl.IsBaseOf(destinationUrl) || _options.Mode == RecursiveProcessingMode.PreferCrossServer)
            {
                // Copy from server to server (slow)
                return new WebDavResult(WebDavStatusCodes.BadGateway);
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
                return await CopyWithinFileSystemAsync(sourceUrl, sourceSelectionResult, targetInfo, overwrite ?? _options.OverwriteAsDefault, cancellationToken).ConfigureAwait(false);
            }

            // Copy one item to another file system (probably slow)
            return await CopyBetweenFileSystemsAsync(sourceUrl, sourceSelectionResult, targetInfo, overwrite ?? _options.OverwriteAsDefault, cancellationToken).ConfigureAwait(false);
        }

        private Task<IWebDavResult> CopyBetweenFileSystemsAsync(
            Uri sourceUrl,
            SelectionResult sourceSelectionResult,
            TargetInfo targetInfo,
            bool overwrite,
            CancellationToken cancellationToken)
        {
            var handler = new CopyFromFileSystemToFileSystemHandler();
            return CopyAsync(handler, sourceUrl, sourceSelectionResult, targetInfo, overwrite, cancellationToken);
        }

        private Task<IWebDavResult> CopyWithinFileSystemAsync(
            Uri sourceUrl,
            SelectionResult sourceSelectionResult,
            TargetInfo targetInfo,
            bool overwrite,
            CancellationToken cancellationToken)
        {
            var handler = new CopyWithinFileSystemHandler();
            return CopyAsync(handler, sourceUrl, sourceSelectionResult, targetInfo, overwrite, cancellationToken);
        }

        private async Task<IWebDavResult> CopyAsync(
            IEntryHandler handler,
            Uri sourceUrl,
            SelectionResult sourceSelectionResult,
            TargetInfo targetInfo,
            bool overwrite,
            CancellationToken cancellationToken)
        {
            Debug.Assert(sourceSelectionResult.Collection != null, "sourceSelectionResult.Collection != null");

            /*
            var engineHandler = new CopyInFileSystemTargetAction();
            var engine = new RecursiveExecutionEngine<CollectionTarget, DocumentTarget, MissingTarget>(
                engineHandler,
                overwrite);

            if (sourceSelectionResult.ResultType == SelectionResultType.FoundDocument)
            {
                var docTarget = DocumentTarget.Create(targetInfo.DestinationUri, (IDocument)targetInfo.TargetEntry, engineHandler);
                var docResult = await engine.ExecuteAsync(
                    sourceUrl,
                    sourceSelectionResult.Document,
                    docTarget,
                    cancellationToken).ConfigureAwait(false);
            }
            */

            //engine.ExecuteAsync(sourceUrl, )

            var processor = new NodeProcessor(handler, overwrite);
            IImmutableList<ElementResult> results;
            if (sourceSelectionResult.Document != null)
            {
                var result = await processor
                    .ExecuteAsync(sourceUrl, sourceSelectionResult.Document, targetInfo, cancellationToken)
                    .ConfigureAwait(false);
                results = ImmutableList<ElementResult>.Empty.Add(result);
            }
            else
            {
                var nodes = await sourceSelectionResult.Collection.GetNodeAsync(Depth.Infinity.OrderValue, cancellationToken).ConfigureAwait(false);
                results = await processor.ExecuteAsync(sourceUrl, nodes, targetInfo, cancellationToken).ConfigureAwait(false);
            }

            var errorResults = results
                .Where(x => x.IsFailure)
                .ToList();

            if (errorResults.Count == 0)
            {
                if (targetInfo.TargetEntry == null)
                    return new WebDavResult(WebDavStatusCodes.Created);
                return new WebDavResult(WebDavStatusCodes.NoContent);
            }

            var resultsByStatus = errorResults.GroupBy(x => x.GetGroupableStatus());
            var responses = new List<Tuple<WebDavStatusCodes, Response>>();
            foreach (var statusResult in resultsByStatus)
            {
                var templateItem = statusResult.First();

                var itemNames = new List<ItemsChoiceType2>();
                var items = new List<object>();

                List<string> hrefs;
                if (templateItem.Error != null)
                {
                    if (templateItem.Error.ItemsElementName.Length != 1)
                        throw new NotSupportedException();

                    var itemElementName = templateItem.Error.ItemsElementName.Single();
                    switch (itemElementName)
                    {
                        case ItemsChoiceType.LockTokenSubmitted:
                        case ItemsChoiceType.NoConflictingLock:
                            hrefs = new List<string>();
                            break;
                        default:
                            hrefs = null;
                            break;
                    }
                }
                else
                {
                    hrefs = null;
                }

                var reasons = new HashSet<string>();
                foreach (var elementResult in statusResult)
                {
                    if (!string.IsNullOrEmpty(elementResult.Reason))
                        reasons.Add(elementResult.Reason);

                    itemNames.Add(ItemsChoiceType2.Href);
                    items.Add(elementResult.Href.ToString());

                    if (elementResult.Error != null && hrefs != null)
                    {
                        var itemsChoiceType = elementResult.Error.ItemsElementName.Single();
                        switch (itemsChoiceType)
                        {
                            case ItemsChoiceType.LockTokenSubmitted:
                                hrefs.AddRange(elementResult.Error.Items.Cast<LockTokenSubmitted>().Single().Href);
                                break;
                            case ItemsChoiceType.NoConflictingLock:
                                hrefs.AddRange(elementResult.Error.Items.Cast<NoConflictingLock>().Single().Href);
                                break;
                        }
                    }
                }

                itemNames.Add(ItemsChoiceType2.Status);
                items.Add($"{_host.RequestProtocol} {(int)templateItem.StatusCode} {templateItem.StatusCode.GetReasonPhrase()}");

                Error error;
                if (templateItem.Error != null)
                {
                    if (templateItem.Error.ItemsElementName.Length != 1)
                        throw new NotSupportedException();

                    var itemElementName = templateItem.Error.ItemsElementName.Single();
                    switch (itemElementName)
                    {
                        case ItemsChoiceType.LockTokenSubmitted:
                            Debug.Assert(hrefs != null, "hrefs != null");
                            error = new Error()
                            {
                                ItemsElementName = new[] {itemElementName},
                                Items = new object[]
                                {
                                    new LockTokenSubmitted()
                                    {
                                        Href = hrefs.ToArray()
                                    }
                                }
                            };
                            break;
                        case ItemsChoiceType.NoConflictingLock:
                            Debug.Assert(hrefs != null, "hrefs != null");
                            error = new Error()
                            {
                                ItemsElementName = new[] {itemElementName},
                                Items = new object[]
                                {
                                    new NoConflictingLock()
                                    {
                                        Href = hrefs.ToArray()
                                    }
                                }
                            };
                            break;
                        default:
                            error = templateItem.Error;
                            break;
                    }
                }
                else
                {
                    error = null;
                }

                var response = new Response()
                {
                    ItemsElementName = itemNames.ToArray(),
                    Items = items.ToArray(),
                    Error = error,
                };

                if (reasons.Count != 0)
                {
                    response.Responsedescription = string.Join(Environment.NewLine, reasons);
                }

                responses.Add(Tuple.Create(templateItem.StatusCode, response));
            }

            if (responses.Count == 1)
            {
                var statusCode = responses.Select(x => x.Item1).Single();
                var errorResponse = responses.Select(x => x.Item2).Single();
                if (errorResponse.Error != null)
                    return new WebDavResult<Error>(statusCode, errorResponse.Error);

                var hrefCount = errorResponse.ItemsElementName.Count(x => x == ItemsChoiceType2.Href) + (errorResponse.Href == null ? 0 : 1);
                if (hrefCount == 1)
                    throw new WebDavException(statusCode);
            }

            return new WebDavResult<Multistatus>(
                WebDavStatusCodes.MultiStatus, 
                new Multistatus()
                {
                    Response = responses.Select(x => x.Item2).ToArray()
                });
        }

        private class NodeProcessor
        {
            private readonly IEntryHandler _handler;
            private readonly bool _allowOverwrite;

            public NodeProcessor(IEntryHandler handler, bool allowOverwrite)
            {
                _handler = handler;
                _allowOverwrite = allowOverwrite;
            }

            public async Task<ElementResult> ExecuteAsync(Uri sourceUrl, IDocument source, TargetInfo targetInfo, CancellationToken cancellationToken)
            {
                if (targetInfo.TargetEntry != null && !_allowOverwrite)
                {
                    return new ElementResult()
                    {
                        Href = targetInfo.DestinationUri,
                        StatusCode = WebDavStatusCodes.PreconditionFailed
                    };
                }

                var properties = await source.GetProperties().ToList(cancellationToken).ConfigureAwait(false);

                IDocument createdDocument;
                if (targetInfo.TargetEntry != null)
                {
                    var targetDocument = targetInfo.TargetEntry as IDocument;
                    if (targetDocument == null)
                    {
                        return new ElementResult()
                        {
                            Href = targetInfo.DestinationUri,
                            StatusCode = WebDavStatusCodes.Conflict,
                            Reason = "Cannot overwrite collection with document"
                        };
                    }

                    if (_handler.ExistingTargetBehaviour == RecursiveTargetBehaviour.DeleteBeforeCopy)
                    {
                        Debug.Assert(targetInfo.ParentEntry != null, "targetInfo.ParentEntry != null");
                        try
                        {
                            await targetDocument.DeleteAsync(cancellationToken).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            return new ElementResult()
                            {
                                Href = targetInfo.DestinationUri,
                                StatusCode = WebDavStatusCodes.Forbidden,
                                Reason = ex.Message,
                            };
                        }

                        try
                        {
                            createdDocument = await _handler
                                .ExecuteAsync(source, targetInfo.ParentEntry, targetInfo.TargetName, cancellationToken)
                                .ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            return new ElementResult()
                            {
                                Href = sourceUrl,
                                StatusCode = WebDavStatusCodes.Conflict,
                                Reason = ex.Message,
                            };
                        }
                    }
                    else
                    {
                        createdDocument = targetDocument;
                        try
                        {
                            using (var inputStream = await source.OpenReadAsync(cancellationToken).ConfigureAwait(false))
                            {
                                using (var outputStream = await targetDocument.CreateAsync(cancellationToken).ConfigureAwait(false))
                                {
                                    await inputStream.CopyToAsync(outputStream, 65536, cancellationToken).ConfigureAwait(false);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            return new ElementResult()
                            {
                                Href = targetInfo.DestinationUri,
                                StatusCode = WebDavStatusCodes.Forbidden,
                                Reason = ex.Message,
                            };
                        }
                    }
                }
                else
                {
                    Debug.Assert(targetInfo.ParentEntry != null, "targetInfo.ParentEntry != null");
                    try
                    {
                        createdDocument = await _handler
                            .ExecuteAsync(source, targetInfo.ParentEntry, targetInfo.TargetName, cancellationToken)
                            .ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        return new ElementResult()
                        {
                            Href = sourceUrl,
                            StatusCode = WebDavStatusCodes.Conflict,
                            Reason = ex.Message,
                        };
                    }
                }

                var result = await SetPropertiesAsync(targetInfo.DestinationUri, createdDocument, properties, cancellationToken).ConfigureAwait(false);
                if (result.StatusCode != WebDavStatusCodes.OK)
                    return result;

                return new ElementResult()
                {
                    Href = targetInfo.DestinationUri,
                    StatusCode = targetInfo.TargetEntry == null ? WebDavStatusCodes.Created : WebDavStatusCodes.NoContent,
                };
            }

            public async Task<IImmutableList<ElementResult>> ExecuteAsync(
                Uri sourceUrl,
                CollectionExtensions.INode sourceNode,
                TargetInfo targetInfo,
                CancellationToken cancellationToken)
            {
                var result = ImmutableList.Create<ElementResult>();

                var targetUrl = targetInfo.DestinationUri;

                ICollection targetCollection;
                if (targetInfo.TargetEntry == null)
                {
                    Debug.Assert(targetInfo.ParentEntry != null, "targetInfo.ParentEntry != null");
                    try
                    {
                        targetCollection = await targetInfo
                            .ParentEntry
                            .CreateCollectionAsync(targetInfo.TargetName, cancellationToken)
                            .ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        result = result.Add(new ElementResult()
                        {
                            Href = targetInfo.DestinationUri,
                            StatusCode = WebDavStatusCodes.Forbidden,
                            Reason = ex.Message,
                        });

                        return result;
                    }
                }
                else
                {
                    targetCollection = targetInfo.TargetEntry as ICollection;
                    if (targetCollection == null)
                    {
                        result = result.Add(new ElementResult()
                        {
                            Href = targetInfo.DestinationUri,
                            StatusCode = WebDavStatusCodes.Conflict,
                            Reason = "Cannot overwrite document with collection"
                        });

                        return result;
                    }
                }

                var properties = await sourceNode.Collection.GetProperties().ToList(cancellationToken).ConfigureAwait(false);

                foreach (var sourceDocument in sourceNode.Documents)
                {
                    var sourceDocumentUrl = sourceUrl.Append(sourceDocument);
                    var targetDocumentUrl = targetUrl.Append(sourceDocument);
                    var targetDocInfo = await TargetInfo
                        .FromDestinationAsync(targetCollection, sourceDocument.Name, targetDocumentUrl, cancellationToken)
                        .ConfigureAwait(false);
                    var docResult = await ExecuteAsync(sourceDocumentUrl, sourceDocument, targetDocInfo, cancellationToken).ConfigureAwait(false);
                    result = result.Add(docResult);
                }

                foreach (var childNode in sourceNode.Nodes)
                {
                    var sourceDocumentUrl = sourceUrl.Append(childNode.Collection.Name, false);
                    var targetDocumentUrl = targetUrl.Append(childNode.Collection.Name, false);
                    var targetNodeInfo = await TargetInfo
                        .FromDestinationAsync(targetCollection, childNode.Collection.Name, targetDocumentUrl, cancellationToken)
                        .ConfigureAwait(false);
                    var nodeResult = await ExecuteAsync(sourceDocumentUrl, childNode, targetNodeInfo, cancellationToken).ConfigureAwait(false);
                    result = result.AddRange(nodeResult);
                }

                try
                {
                    await _handler.ExecuteAsync(sourceNode.Collection, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    result = result.Add(new ElementResult()
                    {
                        Href = sourceUrl,
                        StatusCode = WebDavStatusCodes.Conflict,
                        Reason = ex.Message,
                    });
                }

                var propResult = await SetPropertiesAsync(targetUrl, targetCollection, properties, cancellationToken).ConfigureAwait(false);
                if (propResult.StatusCode != WebDavStatusCodes.OK)
                    result = result.Add(propResult);

                if (targetInfo.TargetEntry == null)
                {
                    result = result.Add(new ElementResult()
                    {
                        Href = targetUrl,
                        StatusCode = WebDavStatusCodes.Created,
                    });
                }
                else
                {
                    result = result.Add(new ElementResult()
                    {
                        Href = targetUrl,
                        StatusCode = WebDavStatusCodes.NoContent,
                    });
                }

                return result;
            }

            private async Task<ElementResult> SetPropertiesAsync(Uri destinationUrl, IEntry destination, IEnumerable<IUntypedReadableProperty> properties, CancellationToken cancellationToken)
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

                var livePropertiesResult = await SetPropertiesAsync(destinationUrl, destination, liveProperties, cancellationToken).ConfigureAwait(false);

                if (deadProperties.Count != 0)
                    await SetPropertiesAsync(destination, deadProperties, cancellationToken).ConfigureAwait(false);

                return livePropertiesResult;
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

            private async Task<ElementResult> SetPropertiesAsync(Uri destinationUrl, IEntry destination, IEnumerable<ILiveProperty> properties, CancellationToken cancellationToken)
            {
                var isPropUsed = new Dictionary<XName, bool>();
                var propNameToValue = new Dictionary<XName, XElement>();
                foreach (var property in properties)
                {
                    propNameToValue[property.Name] = await property.GetXmlValueAsync(cancellationToken).ConfigureAwait(false);
                    isPropUsed[property.Name] = false;
                }

                if (propNameToValue.Count == 0)
                {
                    return new ElementResult()
                    {
                        Href = destinationUrl,
                        StatusCode = WebDavStatusCodes.NoContent
                    };
                }

                using (var propEnum = destination.GetProperties().GetEnumerator())
                {
                    while (await propEnum.MoveNext(cancellationToken).ConfigureAwait(false))
                    {
                        isPropUsed[propEnum.Current.Name] = true;
                        var prop = propEnum.Current as IUntypedWriteableProperty;
                        XElement propValue;
                        if (prop != null && propNameToValue.TryGetValue(prop.Name, out propValue))
                        {
                            await prop.SetXmlValueAsync(propValue, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }

                var hasUnsetLiveProperties = isPropUsed.Any(x => !x.Value);
                if (hasUnsetLiveProperties)
                {
                    var unsetPropNames = isPropUsed.Where(x => !x.Value).Select(x => x.Key.ToString());
                    var unsetProperties = $"The following properties couldn't be set: {string.Join(", ", unsetPropNames)}";
                    return new ElementResult()
                    {
                        Href = destinationUrl,
                        Error = new Error()
                        {
                            ItemsElementName = new[] {ItemsChoiceType.PreservedLiveProperties},
                            Items = new[] {new object()},
                        },
                        Reason = unsetProperties,
                        StatusCode = WebDavStatusCodes.Conflict
                    };
                }

                return new ElementResult()
                {
                    Href = destinationUrl,
                    StatusCode = WebDavStatusCodes.OK
                };
            }
        }

        private class CopyFromFileSystemToFileSystemHandler : IEntryHandler
        {
            public RecursiveTargetBehaviour ExistingTargetBehaviour { get; } = RecursiveTargetBehaviour.Overwrite;

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
            public RecursiveTargetBehaviour ExistingTargetBehaviour { get; } = RecursiveTargetBehaviour.DeleteBeforeCopy;

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
            RecursiveTargetBehaviour ExistingTargetBehaviour { get; }

            [NotNull, ItemNotNull]
            Task<IDocument> ExecuteAsync([NotNull] IDocument source, [NotNull] ICollection destinationParent, [NotNull] string destinationName, CancellationToken cancellationToken);

            [NotNull, ItemNotNull]
            Task<IDocument> ExecuteAsync([NotNull] IDocument source, [NotNull] IDocument destination, CancellationToken cancellationToken);

            [NotNull]
            Task ExecuteAsync([NotNull] ICollection source, CancellationToken cancellationToken);
        }

        private struct TargetInfo
        {
            private TargetInfo([CanBeNull] ICollection parentEntry, [CanBeNull] IEntry targeEntry, [NotNull] string targetName, [NotNull] Uri destinationUri)
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

            public static async Task<TargetInfo> FromDestinationAsync([NotNull] ICollection destination, [NotNull] string name, [NotNull] Uri destinationUri, CancellationToken cancellationToken)
            {
                var targetEntry = await destination.GetChildAsync(name, cancellationToken).ConfigureAwait(false);
                return new TargetInfo(destination, targetEntry, name, destinationUri);
            }
        }

        private struct ElementResult
        {
            public Uri Href { get; set; }
            public WebDavStatusCodes StatusCode { get; set; }
            public Error Error { get; set; }
            public string Reason { get; set; }

            public bool IsFailure => ((int) StatusCode) >= 300;

            public string GetGroupableStatus()
            {
                var result = new StringBuilder()
                    .Append((int)StatusCode);

                if (Error != null)
                {
                    result.Append("+error");
                    for (var i = 0; i != Error.ItemsElementName.Length; ++i)
                    {
                        string textToAppend;
                        switch (Error.ItemsElementName[i])
                        {
                            case ItemsChoiceType.Any:
                                textToAppend = ((XElement) Error.Items[i]).ToString(SaveOptions.OmitDuplicateNamespaces | SaveOptions.DisableFormatting);
                                break;
                            default:
                                textToAppend = Error.ItemsElementName[i].ToString();
                                break;
                        }

                        result.Append(':').Append(Uri.EscapeDataString(textToAppend));
                    }
                }

                return result.ToString();
            }
        }
    }
}
