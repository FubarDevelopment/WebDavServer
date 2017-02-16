// <copyright file="RemoteHttpClientTargetActions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.Props;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines.Remote
{
    public abstract class RemoteHttpClientTargetActions : IRemoteTargetActions
    {
        [NotNull]
        private static readonly Encoding _defaultEncoding = new UTF8Encoding(false);

        [NotNull]
        private static readonly HttpMethod _propFindHttpMethod = new HttpMethod("PROPFIND");

        [NotNull]
        private static readonly HttpMethod _propPatchHttpMethod = new HttpMethod("PROPPATCH");

        [NotNull]
        private static readonly HttpMethod _mkColHttpMethod = new HttpMethod("MKCOL");

        [NotNull]
        private static readonly XmlSerializer _errorSerializer = new XmlSerializer(typeof(error));

        [NotNull]
        private static readonly XmlSerializer _multiStatusSerializer = new XmlSerializer(typeof(multistatus));

        [NotNull]
        private static readonly XmlSerializer _propFindSerializer = new XmlSerializer(typeof(propfind));

        [NotNull]
        private static readonly XmlSerializer _propertyUpdateSerializer = new XmlSerializer(typeof(propertyupdate));

        protected RemoteHttpClientTargetActions([NotNull] HttpClient httpClient)
        {
            Client = httpClient;
        }

        public RecursiveTargetBehaviour ExistingTargetBehaviour { get; } = RecursiveTargetBehaviour.DeleteTarget;

        [NotNull]
        protected HttpClient Client { get; }

        public abstract Task<RemoteDocumentTarget> ExecuteAsync(IDocument source, RemoteMissingTarget destination, CancellationToken cancellationToken);

        public abstract Task<ActionResult> ExecuteAsync(IDocument source, RemoteDocumentTarget destination, CancellationToken cancellationToken);

        public abstract Task ExecuteAsync(ICollection source, RemoteCollectionTarget destination, CancellationToken cancellationToken);

        public Task<IReadOnlyCollection<XName>> SetPropertiesAsync(RemoteCollectionTarget target, IEnumerable<IUntypedWriteableProperty> properties, CancellationToken cancellationToken)
        {
            return SetPropertiesAsync(target.DestinationUrl, properties, cancellationToken);
        }

        public Task<IReadOnlyCollection<XName>> SetPropertiesAsync(RemoteDocumentTarget target, IEnumerable<IUntypedWriteableProperty> properties, CancellationToken cancellationToken)
        {
            return SetPropertiesAsync(target.DestinationUrl, properties, cancellationToken);
        }

        public async Task<RemoteCollectionTarget> CreateCollectionAsync(
            RemoteCollectionTarget collection,
            string name,
            CancellationToken cancellationToken)
        {
            var targetUrl = collection.DestinationUrl.AppendDirectory(name);
            using (var httpRequest = new HttpRequestMessage(_mkColHttpMethod, targetUrl))
            {
                using (var httpResponse = await Client.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false))
                {
                    if (!httpResponse.IsSuccessStatusCode)
                        throw new RemoteTargetException("Failed to create");

                    var resultDoc = await ReadResponseAsync(httpResponse).ConfigureAwait(false);
                    if (resultDoc == null)
                    {
                        httpResponse.EnsureSuccessStatusCode();
                        return new RemoteCollectionTarget(collection, name, targetUrl, true, this);
                    }

                    var result = Parse(targetUrl, httpResponse, resultDoc);
                    if (result.response.Length == 0)
                        return new RemoteCollectionTarget(collection, name, targetUrl, true, this);

                    if (result.response.Length > 1)
                        throw new RemoteTargetException("Received more than one multi-status response", targetUrl);

                    var response = result.response[0];

                    var hrefs = response.GetHrefs().Select(x => new Uri(x, UriKind.RelativeOrAbsolute)).ToList();
                    if (hrefs.Count == 0)
                        hrefs.Add(targetUrl);

                    if (response.error != null)
                        throw CreateException(targetUrl, response.error);

                    var statusIndex = Array.IndexOf(response.ItemsElementName, ItemsChoiceType2.status);
                    if (statusIndex == -1)
                        return new RemoteCollectionTarget(collection, name, targetUrl, true, this);

                    var status = Status.Parse((string)response.Items[statusIndex]);
                    if (!status.IsSuccessStatusCode)
                        throw new RemoteTargetException(status.ToString(), hrefs);

                    return new RemoteCollectionTarget(collection, name, targetUrl, true, this);
                }
            }
        }

        public async Task<ITarget> GetAsync(RemoteCollectionTarget collection, string name, CancellationToken cancellationToken)
        {
            var requestData = new propfind()
            {
                ItemsElementName = new[] { ItemsChoiceType1.prop, },
                Items = new object[]
                {
                    new prop()
                    {
                        Any = new[]
                        {
                            new XElement(Props.Live.ResourceTypeProperty.PropertyName),
                        },
                    },
                },
            };

            multistatus result;

            var targetUrl = collection.DestinationUrl.Append(name, false);
            using (var httpRequest = new HttpRequestMessage(_propFindHttpMethod, targetUrl)
            {
                Headers =
                {
                    { "Depth", Depth.Zero.Value },
                },
                Content = CreateContent(_propFindSerializer, requestData),
            })
            {
                using (var httpResponse = await Client.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false))
                {
                    if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                        return new RemoteMissingTarget(collection, targetUrl, name, this);

                    var resultDoc = await ReadResponseAsync(httpResponse).ConfigureAwait(false);
                    if (resultDoc == null)
                        throw new RemoteTargetException("The destination server didn't return a response", targetUrl);

                    result = Parse(targetUrl, httpResponse, resultDoc);
                }
            }

            if (result.response == null || result.response.Length == 0)
                throw new RemoteTargetException("The destination server didn't return a response", targetUrl);
            if (result.response.Length != 1)
                throw new RemoteTargetException("Received more than one multi-status response", targetUrl);

            var response = result.response[0];

            var hrefs = response.GetHrefs().Select(x => new Uri(x, UriKind.RelativeOrAbsolute)).ToList();
            if (hrefs.Count == 0)
                hrefs.Add(targetUrl);

            var statusIndex = Array.IndexOf(response.ItemsElementName, ItemsChoiceType2.status);
            var responseStatus = GetStatusCode(
                response.error,
                statusIndex == -1 ? null : (string)response.Items[statusIndex],
                targetUrl,
                hrefs);
            if (responseStatus == (int)WebDavStatusCode.NotFound)
                return new RemoteMissingTarget(collection, targetUrl, name, this);

            var propStatIndex = Array.IndexOf(response.ItemsElementName, ItemsChoiceType2.propstat);
            if (propStatIndex == -1)
                throw new RemoteTargetException("No result returned", hrefs);

            var propStat = (propstat)response.Items[propStatIndex];
            var location = string.IsNullOrEmpty(propStat.location?.href) ? targetUrl : new Uri(propStat.location.href, UriKind.RelativeOrAbsolute);
            var propStatus = GetStatusCode(propStat.error, propStat.status, location, hrefs);
            if (propStatus == (int)WebDavStatusCode.NotFound)
                return new RemoteMissingTarget(collection, targetUrl, name, this);

            var resourceType = propStat
                .prop.Any
                .SingleOrDefault(x => x.Name == Props.Live.ResourceTypeProperty.PropertyName);
            var collectionElement = resourceType?.Element(WebDavXml.Dav + "collection");
            if (collectionElement == null)
                return new RemoteDocumentTarget(collection, name, targetUrl, this);

            return new RemoteCollectionTarget(collection, name, collection.DestinationUrl.AppendDirectory(name), false, this);
        }

        public async Task DeleteAsync(RemoteCollectionTarget target, CancellationToken cancellationToken)
        {
            using (var response = await Client.DeleteAsync(target.DestinationUrl, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task DeleteAsync(RemoteDocumentTarget target, CancellationToken cancellationToken)
        {
            using (var response = await Client.DeleteAsync(target.DestinationUrl, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
            }
        }

        public void Dispose()
        {
            Client.Dispose();
        }

        [NotNull]
        protected static RemoteTargetException CreateException([NotNull] Uri requestUrl, [NotNull] error error)
        {
            var hrefs = new List<Uri>();
            string message = "Unknown error";
            var index = 0;
            foreach (var choiceType in error.ItemsElementName)
            {
                var choiceItem = error.Items[index++];
                switch (choiceType)
                {
                    case ItemsChoiceType.Item:
                        message = "Request failed with element {choiceItem}";
                        break;
                    case ItemsChoiceType.cannotmodifyprotectedproperty:
                        message = "Request tried to modify a protected property";
                        break;
                    case ItemsChoiceType.locktokenmatchesrequesturi:
                        message = "No lock token found for the given request URI";
                        break;
                    case ItemsChoiceType.locktokensubmitted:
                        message = "Locked resource found";
                        hrefs.AddRange(((errorLocktokensubmitted)choiceItem).href?.Select(x => new Uri(x, UriKind.RelativeOrAbsolute)) ?? new Uri[0]);
                        break;
                    case ItemsChoiceType.noconflictinglock:
                        message = "Conflicting lock";
                        hrefs.AddRange(((errorNoconflictinglock)choiceItem).href?.Select(x => new Uri(x, UriKind.RelativeOrAbsolute)) ?? new Uri[0]);
                        break;
                    case ItemsChoiceType.noexternalentities:
                        message = "External XML entities unsupported";
                        break;
                    case ItemsChoiceType.preservedliveproperties:
                        message = "Request failed to modify a live property";
                        break;
                    case ItemsChoiceType.propfindfinitedepth:
                        message = "The server doesn't support infinite depth";
                        break;
                    default:
                        message = "Unknown error";
                        break;
                }
            }

            if (hrefs.Count == 0)
                hrefs.Add(requestUrl);

            return new RemoteTargetException(message, hrefs);
        }

        protected multistatus Parse([NotNull] Uri requrestUrl, [NotNull] HttpResponseMessage responseMessage, [CanBeNull] XDocument document)
        {
            if (document == null)
            {
                var status = new Status($"HTTP/{responseMessage.Version}", responseMessage.StatusCode, responseMessage.ReasonPhrase);
                return new multistatus
                {
                    response = new[]
                    {
                        new response()
                        {
                            href = requrestUrl.ToString(),
                            ItemsElementName = new[] { ItemsChoiceType2.status, },
                            Items = new object[] { status.ToString() },
                        },
                    },
                };
            }

            var errorName = WebDavXml.Dav + "error";
            Debug.Assert(document.Root != null, "document.Root != null");
            if (document.Root.Name == errorName)
            {
                var error = (error)_errorSerializer.Deserialize(document.Root.CreateReader());
                var status = new Status($"HTTP/{responseMessage.Version}", responseMessage.StatusCode, responseMessage.ReasonPhrase);
                return new multistatus
                {
                    response = new[]
                    {
                        new response()
                        {
                            href = requrestUrl.ToString(),
                            ItemsElementName = new[] { ItemsChoiceType2.status, },
                            Items = new object[] { status.ToString() },
                            error = error,
                        },
                    },
                };
            }

            var result = (multistatus)_multiStatusSerializer.Deserialize(document.CreateReader());
            return result;
        }

        [NotNull]
        [ItemCanBeNull]
        protected async Task<XDocument> ReadResponseAsync([NotNull] HttpResponseMessage responseMessage)
        {
            var content = responseMessage.Content;
            if (content == null)
                return null;

            Encoding encoding = null;
            if (content.Headers.ContentType != null)
            {
                switch (content.Headers.ContentType.MediaType)
                {
                    case "application/xml":
                    case "text/xml":
                        if (!string.IsNullOrEmpty(content.Headers.ContentType.CharSet))
                            encoding = Encoding.GetEncoding(content.Headers.ContentType.CharSet);
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported media type {content.Headers.ContentType.MediaType}");
                }
            }

            var buffer = await content.ReadAsByteArrayAsync().ConfigureAwait(false);
            if (buffer.Length == 0)
                return null;

            using (var input = new MemoryStream(buffer))
            {
                if (encoding == null)
                {
                    return XDocument.Load(input);
                }

                using (var reader = new StreamReader(input, encoding))
                {
                    return XDocument.Load(reader);
                }
            }
        }

        private static int GetStatusCode([CanBeNull] error error, [CanBeNull] string statusLine, [NotNull] Uri targetUrl, [NotNull][ItemNotNull] IReadOnlyCollection<Uri> hrefs)
        {
            if (error != null)
                throw CreateException(targetUrl, error);

            if (string.IsNullOrEmpty(statusLine))
                return (int)WebDavStatusCode.OK;

            var status = Status.Parse(statusLine);
            if (!status.IsSuccessStatusCode && status.StatusCode != (int)WebDavStatusCode.NotFound)
                throw new RemoteTargetException(status.ToString(), hrefs);

            return status.StatusCode;
        }

        private static HttpContent CreateContent([NotNull] XmlSerializer serializer, [NotNull] object requestData)
        {
            byte[] data;
            using (var requestStream = new MemoryStream())
            {
                using (var requestWriter = new StreamWriter(requestStream, _defaultEncoding))
                {
                    serializer.Serialize(requestStream, requestData);
                    requestWriter.Flush();
                    data = requestStream.ToArray();
                }
            }

            var content = new ByteArrayContent(data)
            {
                Headers =
                {
                    ContentType = MediaTypeHeaderValue.Parse($"text/xml; charset={_defaultEncoding.WebName}"),
                },
            };

            return content;
        }

        private async Task<IReadOnlyCollection<XName>> SetPropertiesAsync([NotNull] Uri targetUrl, [NotNull][ItemNotNull] IEnumerable<IUntypedWriteableProperty> properties, CancellationToken cancellationToken)
        {
            var elements = new List<XElement>();

            foreach (var property in properties)
            {
                var element = await property.GetXmlValueAsync(cancellationToken).ConfigureAwait(false);
                elements.Add(element);
            }

            var requestData = new propertyupdate()
            {
                Items = new object[]
                {
                    new propset()
                    {
                        prop = new prop()
                        {
                            Any = elements.ToArray(),
                        },
                    },
                },
            };

            multistatus result;

            using (var httpRequest = new HttpRequestMessage(_propPatchHttpMethod, targetUrl)
            {
                Content = CreateContent(_propertyUpdateSerializer, requestData),
            })
            {
                using (var httpResponse = await Client.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false))
                {
                    if (httpResponse.IsSuccessStatusCode)
                        return new XName[0];

                    var resultDoc = await ReadResponseAsync(httpResponse).ConfigureAwait(false);
                    if (resultDoc == null)
                        throw new RemoteTargetException("The destination server didn't return a response", targetUrl);

                    result = Parse(targetUrl, httpResponse, resultDoc);
                }
            }

            if (result.response == null || result.response.Length == 0)
                throw new RemoteTargetException("The destination server didn't return a response", targetUrl);
            if (result.response.Length != 1)
                throw new RemoteTargetException("Received more than one multi-status response", targetUrl);

            var response = result.response[0];

            var hrefs = response.GetHrefs().Select(x => new Uri(x, UriKind.RelativeOrAbsolute)).ToList();
            if (hrefs.Count == 0)
                hrefs.Add(targetUrl);

            var statusIndex = Array.IndexOf(response.ItemsElementName, ItemsChoiceType2.status);
            var isFailure = response.error != null;
            if (statusIndex != -1 && !isFailure)
            {
                var responseStatus = Status.Parse((string)response.Items[statusIndex]);
                isFailure = !responseStatus.IsSuccessStatusCode;
            }

            var hasFailedPropStats = false;
            var failedProperties = new List<XName>();
            foreach (var propstat in response.Items.OfType<propstat>())
            {
                var propStatIsFailure =
                    isFailure
                    || propstat.error != null
                    || (!string.IsNullOrEmpty(propstat.status) && !Status.Parse(propstat.status).IsSuccessStatusCode);
                hasFailedPropStats |= propStatIsFailure;
                if (propStatIsFailure && propstat.prop?.Any != null)
                {
                    foreach (var element in propstat.prop.Any)
                    {
                        failedProperties.Add(element.Name);
                    }
                }
            }

            if (failedProperties.Count == 0 && (isFailure || hasFailedPropStats))
                throw new RemoteTargetException("Failed properties were not returned by the server.");

            return failedProperties;
        }
    }
}
