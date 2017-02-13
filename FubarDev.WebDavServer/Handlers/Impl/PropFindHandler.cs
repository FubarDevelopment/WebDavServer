// <copyright file="PropFindHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Props.Filters;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    public class PropFindHandler : IPropFindHandler
    {
        private readonly IWebDavHost _host;

        public PropFindHandler(IFileSystem fileSystem, IWebDavHost host)
        {
            _host = host;
            FileSystem = fileSystem;
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; } = new[] { "PROPFIND" };

        public IFileSystem FileSystem { get; }

        /// <inheritdoc />
        public async Task<IWebDavResult> PropFindAsync(string path, Propfind request, Depth depth, CancellationToken cancellationToken)
        {
            var selectionResult = await FileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);
            if (selectionResult.IsMissing)
            {
                throw new WebDavException(WebDavStatusCode.NotFound);
            }

            var entries = new List<IEntry>();
            if (selectionResult.ResultType == SelectionResultType.FoundDocument)
            {
                entries.Add(selectionResult.Document);
            }
            else
            {
                Debug.Assert(selectionResult.Collection != null, "selectionResult.Collection != null");
                Debug.Assert(selectionResult.ResultType == SelectionResultType.FoundCollection, "selectionResult.ResultType == SelectionResultType.FoundCollection");
                entries.Add(selectionResult.Collection);
                if (depth == Depth.One)
                {
                    entries.AddRange(await selectionResult.Collection.GetChildrenAsync(cancellationToken).ConfigureAwait(false));
                }
                else if (depth == Depth.Infinity)
                {
                    var collector = selectionResult.Collection as IRecusiveChildrenCollector;
                    if (collector == null)
                    {
                        // Cannot recursively collect the children with infinite depth
                        return new WebDavResult<Error>(WebDavStatusCode.Forbidden, new Error()
                        {
                            ItemsElementName = new[] { ItemsChoiceType.PropfindFiniteDepth, },
                            Items = new[] { new object(), },
                        });
                    }

                    var remainingDepth = depth.OrderValue - (depth != Depth.Infinity ? 1 : 0);
                    using (var entriesEnumerator = collector.GetEntries(remainingDepth).GetEnumerator())
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
                case ItemsChoiceType1.Allprop:
                    return await HandleAllPropAsync(request, entries, cancellationToken).ConfigureAwait(false);
                case ItemsChoiceType1.Prop:
                    return await HandlePropAsync((Prop)request.Items[0], entries, cancellationToken).ConfigureAwait(false);
                case ItemsChoiceType1.Propname:
                    return await HandlePropNameAsync(entries, cancellationToken).ConfigureAwait(false);
            }

            throw new WebDavException(WebDavStatusCode.Forbidden);
        }

        private async Task<IWebDavResult> HandlePropAsync(Prop prop, IReadOnlyCollection<IEntry> entries, CancellationToken cancellationToken)
        {
            var responses = new List<Response>();
            foreach (var entry in entries)
            {
                var href = _host.BaseUrl.Append(entry.Path);

                var collector = new PropertyCollector(_host, new ReadableFilter(), new PropFilter(prop));
                var propStats = await collector.GetPropertiesAsync(entry, int.MaxValue, cancellationToken).ConfigureAwait(false);

                var response = new Response()
                {
                    Href = href.OriginalString,
                    ItemsElementName = propStats.Select(x => ItemsChoiceType2.Propstat).ToArray(),
                    Items = propStats.Cast<object>().ToArray(),
                };

                responses.Add(response);
            }

            var result = new Multistatus()
            {
                Response = responses.ToArray(),
            };

            return new WebDavResult<Multistatus>(WebDavStatusCode.MultiStatus, result);
        }

        private Task<IWebDavResult> HandleAllPropAsync([NotNull] Propfind request, IEnumerable<IEntry> entries, CancellationToken cancellationToken)
        {
            var include = request.ItemsElementName.Select((x, i) => Tuple.Create(x, i)).Where(x => x.Item1 == ItemsChoiceType1.Include).Select(x => (Include)request.Items[x.Item2]).FirstOrDefault();
            return HandleAllPropAsync(include, entries, cancellationToken);
        }

        private Task<IWebDavResult> HandleAllPropAsync(IEnumerable<IEntry> entries, CancellationToken cancellationToken)
        {
            return HandleAllPropAsync((Include)null, entries, cancellationToken);
        }

        // ReSharper disable once UnusedParameter.Local
        private async Task<IWebDavResult> HandleAllPropAsync([CanBeNull] Include include, IEnumerable<IEntry> entries, CancellationToken cancellationToken)
        {
            var responses = new List<Response>();
            foreach (var entry in entries)
            {
                var entryPath = entry.Path.OriginalString.TrimEnd('/');
                var href = _host.BaseUrl.Append(entryPath, true);

                var collector = new PropertyCollector(_host, new ReadableFilter(), new CostFilter(0));
                var propStats = await collector.GetPropertiesAsync(entry, 0, cancellationToken).ConfigureAwait(false);

                var response = new Response()
                {
                    Href = href.OriginalString,
                    ItemsElementName = propStats.Select(x => ItemsChoiceType2.Propstat).ToArray(),
                    Items = propStats.Cast<object>().ToArray(),
                };

                responses.Add(response);
            }

            var result = new Multistatus()
            {
                Response = responses.ToArray(),
            };

            return new WebDavResult<Multistatus>(WebDavStatusCode.MultiStatus, result);
        }

        private async Task<IWebDavResult> HandlePropNameAsync(IEnumerable<IEntry> entries, CancellationToken cancellationToken)
        {
            var responses = new List<Response>();
            foreach (var entry in entries)
            {
                var entryPath = entry.Path.OriginalString.TrimEnd('/');
                var href = _host.BaseUrl.Append(entryPath, true);

                var collector = new PropertyCollector(_host, new ReadableFilter(), new CostFilter(0));
                var propStats = await collector.GetPropertyNamesAsync(entry, cancellationToken).ConfigureAwait(false);

                var response = new Response()
                {
                    Href = href.OriginalString,
                    ItemsElementName = propStats.Select(x => ItemsChoiceType2.Propstat).ToArray(),
                    Items = propStats.Cast<object>().ToArray(),
                };

                responses.Add(response);
            }

            var result = new Multistatus()
            {
                Response = responses.ToArray(),
            };

            return new WebDavResult<Multistatus>(WebDavStatusCode.MultiStatus, result);
        }

        private class PropertyCollector
        {
            private readonly IWebDavHost _host;

            private readonly IPropertyFilter[] _filters;

            public PropertyCollector(IWebDavHost host, params IPropertyFilter[] filters)
            {
                _host = host;
                _filters = filters;
            }

            public async Task<IReadOnlyCollection<Propstat>> GetPropertiesAsync(IEntry entry, int maxCost, CancellationToken cancellationToken)
            {
                foreach (var filter in _filters)
                {
                    filter.Reset();
                }

                var propElements = new List<XElement>();
                using (var propsEnumerator = entry.GetProperties(maxCost).GetEnumerator())
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
                            Status = new Status(_host.RequestProtocol, WebDavStatusCode.OK).ToString(),
                        });
                }

                var missingProperties = _filters
                    .SelectMany(x => x.GetMissingProperties())
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
                            Status = new Status(_host.RequestProtocol, item.Key).ToString(),
                        });
                }

                return result;
            }

            public async Task<IReadOnlyCollection<Propstat>> GetPropertyNamesAsync(IEntry entry, CancellationToken cancellationToken)
            {
                foreach (var filter in _filters)
                {
                    filter.Reset();
                }

                var propElements = new List<XElement>();
                using (var propsEnumerator = entry.GetProperties(int.MaxValue).GetEnumerator())
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
                        var element = new XElement(readableProp.Name);
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
                            Status = new Status(_host.RequestProtocol, WebDavStatusCode.OK).ToString(),
                        });
                }

                var missingProperties = _filters
                    .SelectMany(x => x.GetMissingProperties())
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
                            Status = new Status(_host.RequestProtocol, item.Key).ToString(),
                        });
                }

                return result;
            }
        }
    }
}
