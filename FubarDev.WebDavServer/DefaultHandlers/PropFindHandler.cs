using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties;
using FubarDev.WebDavServer.Properties.Filters;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.DefaultHandlers
{
    public class PropFindHandler : IPropFindHandler
    {
        private readonly IWebDavHost _host;

        public PropFindHandler(IFileSystem fileSystem, IWebDavHost host)
        {
            _host = host;
            FileSystem = fileSystem;
        }

        public IEnumerable<string> HttpMethods { get; } = new[] { "PROPFIND" };

        public IFileSystem FileSystem { get; }

        public async Task<IWebDavResult> PropFindAsync(string path, Propfind request, Depth depth, CancellationToken cancellationToken)
        {
            if (depth == Depth.Infinity)
            {
                // Not supported yet
                return new WebDavResult<Error1>(WebDavStatusCodes.Forbidden, new Error1()
                {
                    ItemsElementName = new[] {ItemsChoiceType2.PropfindFiniteDepth,},
                    Items = new[] {new object(),}
                });
            }

            var selectionResult = await FileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);
            if (selectionResult.IsMissing)
            {
                throw new WebDavException(WebDavStatusCodes.NotFound);
            }

            var entries = new List<IEntry>();
            if (selectionResult.ResultType == SelectionResultType.FoundDocument)
            {
                entries.Add(selectionResult.Document);
            }
            else
            {
                Debug.Assert(selectionResult.ResultType == SelectionResultType.FoundCollection);
                entries.Add(selectionResult.Collection);
                if (depth != Depth.Zero)
                {
                    var children = await selectionResult.Collection.GetChildrenAsync(cancellationToken).ConfigureAwait(false);

                    using (var entriesEnumerator = selectionResult.Collection.GetEntries(children, depth.OrderValue - 1).GetEnumerator())
                    {
                        while (await entriesEnumerator.MoveNext(cancellationToken).ConfigureAwait(false))
                        {
                            entries.Add(entriesEnumerator.Current);
                        }
                    }
                }
            }

            if (request == null)
                return await HandleAllPropAsync(entries, cancellationToken).ConfigureAwait(false);

            switch (request.ItemsElementName[0])
            {
                case ItemsChoiceType.Allprop:
                    return await HandleAllPropAsync(request, entries, cancellationToken).ConfigureAwait(false);
                case ItemsChoiceType.Prop:
                    return await HandlePropAsync((Prop)request.Items[0], entries, cancellationToken).ConfigureAwait(false);
            }

            throw new WebDavException(WebDavStatusCodes.Forbidden);
        }

        private async Task<IWebDavResult> HandlePropAsync(Prop prop, IReadOnlyCollection<IEntry> entries, CancellationToken cancellationToken)
        {
            var responses = new List<Response>();
            foreach (var entry in entries)
            {
                var href = new Uri(_host.BaseUrl, entry.Path);

                var collector = new PropertyCollector(_host, new ReadableFilter(), new PropFilter(prop));
                var propStats = await collector.GetPropertiesAsync(entry, code => code != WebDavStatusCodes.NotFound, cancellationToken).ConfigureAwait(false);

                var response = new Response()
                {
                    Href = href.GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped),
                    ItemsElementName = propStats.Select(x => ItemsChoiceType1.Propstat).ToArray(),
                    Items = propStats.Cast<object>().ToArray(),
                };

                responses.Add(response);
            }

            var result = new Multistatus()
            {
                Response = responses.ToArray()
            };

            return new WebDavResult<Multistatus>(WebDavStatusCodes.MultiStatus, result);
        }

        private Task<IWebDavResult> HandleAllPropAsync([NotNull] Propfind request, IReadOnlyCollection<IEntry> entries, CancellationToken cancellationToken)
        {
            var include = request.ItemsElementName.Select((x, i) => Tuple.Create(x, i)).Where(x => x.Item1 == ItemsChoiceType.Include).Select(x => (Include)request.Items[x.Item2]).FirstOrDefault();
            return HandleAllPropAsync(include, entries, cancellationToken);
        }

        private Task<IWebDavResult> HandleAllPropAsync(IReadOnlyCollection<IEntry> entries, CancellationToken cancellationToken)
        {
            return HandleAllPropAsync((Include)null, entries, cancellationToken);
        }

        private async Task<IWebDavResult> HandleAllPropAsync([CanBeNull] Include include, IReadOnlyCollection<IEntry> entries, CancellationToken cancellationToken)
        {
            var responses = new List<Response>();
            foreach (var entry in entries)
            {
                var entryPath = entry.Path.TrimEnd('/');
                var href = new Uri(_host.BaseUrl, entryPath);

                var collector = new PropertyCollector(_host, new ReadableFilter(), new CostFilter(0));
                var propStats = await collector.GetPropertiesAsync(entry, cancellationToken).ConfigureAwait(false);

                var response = new Response()
                {
                    Href = href.GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped),
                    ItemsElementName = propStats.Select(x => ItemsChoiceType1.Propstat).ToArray(),
                    Items = propStats.Cast<object>().ToArray(),
                };

                responses.Add(response);
            }

            var result = new Multistatus()
            {
                Response = responses.ToArray()
            };

            return new WebDavResult<Multistatus>(WebDavStatusCodes.MultiStatus, result);
        }

        class PropertyCollector
        {
            private readonly IWebDavHost _host;

            private readonly IPropertyFilter[] _filters;

            public PropertyCollector(IWebDavHost host, params IPropertyFilter[] filters)
            {
                _host = host;
                _filters = filters;
            }

            public Task<IReadOnlyCollection<Propstat>> GetPropertiesAsync(IEntry entry, CancellationToken cancellationToken)
            {
                return GetPropertiesAsync(entry, code => code != WebDavStatusCodes.NotFound, cancellationToken);
            }

            public async Task<IReadOnlyCollection<Propstat>> GetPropertiesAsync(IEntry entry, Func<WebDavStatusCodes, bool> statusCodeFilter, CancellationToken cancellationToken)
            {
                foreach (var filter in _filters)
                {
                    filter.Reset();
                }

                var propElements = new List<XElement>();
                using (var propsEnumerator = entry.GetProperties().GetEnumerator())
                {
                    while (await propsEnumerator.MoveNext(cancellationToken).ConfigureAwait(false))
                    {
                        var property = propsEnumerator.Current;

                        if (!_filters.All(x => x.IsAllowed(property)))
                            continue;

                        foreach (var filter in _filters)
                        {
                            filter.NotifyOfSelection(property);
                        }

                        var readableProp = property;
                        var element = await readableProp.GetXmlValueAsync(cancellationToken).ConfigureAwait(false);
                        propElements.Add(element);
                    }
                }

                var result = new List<Propstat>();
                if (propElements.Count != 0)
                {
                    result.Add(
                        new Propstat()
                        {
                            Prop = new Prop()
                            {
                                Any = propElements.ToArray(),
                            },
                            Status = $"{_host.RequestProtocol} 200 OK"
                        });
                }

                var missingProperties = _filters
                    .SelectMany(x => x.GetMissingProperties())
                    .Where(x => statusCodeFilter(x.StatusCode))
                    .GroupBy(x => x.StatusCode, x => x.PropertyName)
                    .ToDictionary(x => x.Key, x => x.Distinct().ToList());
                foreach (var item in missingProperties)
                {
                    result.Add(
                        new Propstat()
                        {
                            Prop = new Prop()
                            {
                                Any = item.Value.Select(x => new XElement(x)).ToArray(),
                            },
                            Status = $"{_host.RequestProtocol} {item.Key} {item.Key.GetReasonPhrase()}"
                        });
                }

                return result;
            }
        }
    }
}
