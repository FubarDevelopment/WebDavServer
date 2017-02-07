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
using FubarDev.WebDavServer.Props;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines.Remote
{
    public abstract class RemoteHttpClientTargetActions : IRemoteTargetActions
    {
        private static readonly Encoding _defaultEncoding = new UTF8Encoding(false);
        private static readonly HttpMethod _propFindHttpMethod = new HttpMethod("PROPFIND");
        private static readonly HttpMethod _propPatchHttpMethod = new HttpMethod("PROPPATCH");
        private static readonly HttpMethod _mkColHttpMethod = new HttpMethod("MKCOL");
        private static readonly XmlSerializer _errorSerializer = new XmlSerializer(typeof(Error));
        private static readonly XmlSerializer _multiStatusSerializer = new XmlSerializer(typeof(Multistatus));
        private static readonly XmlSerializer _propFindSerializer = new XmlSerializer(typeof(Propfind));
        private static readonly XmlSerializer _propertyUpdateSerializer = new XmlSerializer(typeof(Propertyupdate));

        protected RemoteHttpClientTargetActions(HttpClient httpClient)
        {
            Client = httpClient;
        }

        public RecursiveTargetBehaviour ExistingTargetBehaviour { get; } = RecursiveTargetBehaviour.DeleteTarget;

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
                    if (result.Response.Length == 0)
                        return new RemoteCollectionTarget(collection, name, targetUrl, true, this);

                    if (result.Response.Length > 1)
                        throw new RemoteTargetException("Received more than one multi-status response", targetUrl);

                    var response = result.Response[0];

                    var hrefs = response.GetHrefs().Select(x => new Uri(x, UriKind.RelativeOrAbsolute)).ToList();
                    if (hrefs.Count == 0)
                        hrefs.Add(targetUrl);

                    if (response.Error != null)
                        throw CreateException(targetUrl, response.Error);

                    var statusIndex = Array.IndexOf(response.ItemsElementName, ItemsChoiceType2.Status);
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
            var requestData = new Propfind()
            {
                ItemsElementName = new[] { ItemsChoiceType1.Prop, },
                Items = new object[]
                {
                    new Prop()
                    {
                        Any = new[]
                        {
                            new XElement(Props.Live.ResourceTypeProperty.PropertyName),
                        },
                    },
                },
            };

            Multistatus result;

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

            if (result.Response == null || result.Response.Length == 0)
                throw new RemoteTargetException("The destination server didn't return a response", targetUrl);
            if (result.Response.Length != 1)
                throw new RemoteTargetException("Received more than one multi-status response", targetUrl);

            var response = result.Response[0];

            var hrefs = response.GetHrefs().Select(x => new Uri(x, UriKind.RelativeOrAbsolute)).ToList();
            if (hrefs.Count == 0)
                hrefs.Add(targetUrl);

            var statusIndex = Array.IndexOf(response.ItemsElementName, ItemsChoiceType2.Status);
            var responseStatus = GetStatusCode(
                response.Error,
                statusIndex == -1 ? null : (string)response.Items[statusIndex],
                targetUrl,
                hrefs);
            if (responseStatus == (int)WebDavStatusCode.NotFound)
                return new RemoteMissingTarget(collection, targetUrl, name, this);

            var propStatIndex = Array.IndexOf(response.ItemsElementName, ItemsChoiceType2.Propstat);
            if (propStatIndex == -1)
                throw new RemoteTargetException("No result returned", hrefs);

            var propStat = (Propstat)response.Items[propStatIndex];
            var location = string.IsNullOrEmpty(propStat.Location?.Href) ? targetUrl : new Uri(propStat.Location.Href, UriKind.RelativeOrAbsolute);
            var propStatus = GetStatusCode(propStat.Error, propStat.Status, location, hrefs);
            if (propStatus == (int)WebDavStatusCode.NotFound)
                return new RemoteMissingTarget(collection, targetUrl, name, this);

            var resourceType = propStat
                .Prop.Any
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
        protected static RemoteTargetException CreateException(Uri requestUrl, [NotNull] Error error)
        {
            var hrefs = new List<Uri>();
            string message = "Unknown error";
            var index = 0;
            foreach (var choiceType in error.ItemsElementName)
            {
                var choiceItem = error.Items[index++];
                switch (choiceType)
                {
                    case ItemsChoiceType.Any:
                        message = "Request failed with element {choiceItem}";
                        break;
                    case ItemsChoiceType.CannotModifyProtectedProperty:
                        message = "Request tried to modify a protected property";
                        break;
                    case ItemsChoiceType.LockTokenMatchesRequestUri:
                        message = "No lock token found for the given request URI";
                        break;
                    case ItemsChoiceType.LockTokenSubmitted:
                        message = "Locked resource found";
                        hrefs.AddRange(((LockTokenSubmitted)choiceItem).Href?.Select(x => new Uri(x, UriKind.RelativeOrAbsolute)) ?? new Uri[0]);
                        break;
                    case ItemsChoiceType.NoConflictingLock:
                        message = "Conflicting lock";
                        hrefs.AddRange(((NoConflictingLock)choiceItem).Href?.Select(x => new Uri(x, UriKind.RelativeOrAbsolute)) ?? new Uri[0]);
                        break;
                    case ItemsChoiceType.NoExternalEntities:
                        message = "External XML entities unsupported";
                        break;
                    case ItemsChoiceType.PreservedLiveProperties:
                        message = "Request failed to modify a live property";
                        break;
                    case ItemsChoiceType.PropfindFiniteDepth:
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

        protected Multistatus Parse(Uri requrestUrl, HttpResponseMessage responseMessage, XDocument document)
        {
            if (document == null)
            {
                var status = new Status($"HTTP/{responseMessage.Version}", responseMessage.StatusCode, responseMessage.ReasonPhrase);
                return new Multistatus
                {
                    Response = new[]
                    {
                        new Response()
                        {
                            Href = requrestUrl.ToString(),
                            ItemsElementName = new[] { ItemsChoiceType2.Status, },
                            Items = new object[] { status.ToString() },
                        },
                    },
                };
            }

            var errorName = WebDavXml.Dav + "error";
            Debug.Assert(document.Root != null, "document.Root != null");
            if (document.Root.Name == errorName)
            {
                var error = (Error)_errorSerializer.Deserialize(document.Root.CreateReader());
                var status = new Status($"HTTP/{responseMessage.Version}", responseMessage.StatusCode, responseMessage.ReasonPhrase);
                return new Multistatus
                {
                    Response = new[]
                    {
                        new Response()
                        {
                            Href = requrestUrl.ToString(),
                            ItemsElementName = new[] { ItemsChoiceType2.Status, },
                            Items = new object[] { status.ToString() },
                            Error = error,
                        },
                    },
                };
            }

            var result = (Multistatus)_multiStatusSerializer.Deserialize(document.CreateReader());
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

        private static int GetStatusCode(Error error, string statusLine, Uri targetUrl, IReadOnlyCollection<Uri> hrefs)
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

        private static HttpContent CreateContent(XmlSerializer serializer, object requestData)
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

        private async Task<IReadOnlyCollection<XName>> SetPropertiesAsync(Uri targetUrl, IEnumerable<IUntypedWriteableProperty> properties, CancellationToken cancellationToken)
        {
            var elements = new List<XElement>();

            foreach (var property in properties)
            {
                var element = await property.GetXmlValueAsync(cancellationToken).ConfigureAwait(false);
                elements.Add(element);
            }

            var requestData = new Propertyupdate()
            {
                Items = new object[]
                {
                    new Propset()
                    {
                        Prop = new Prop()
                        {
                            Any = elements.ToArray(),
                        },
                    },
                },
            };

            Multistatus result;

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

            if (result.Response == null || result.Response.Length == 0)
                throw new RemoteTargetException("The destination server didn't return a response", targetUrl);
            if (result.Response.Length != 1)
                throw new RemoteTargetException("Received more than one multi-status response", targetUrl);

            var response = result.Response[0];

            var hrefs = response.GetHrefs().Select(x => new Uri(x, UriKind.RelativeOrAbsolute)).ToList();
            if (hrefs.Count == 0)
                hrefs.Add(targetUrl);

            var statusIndex = Array.IndexOf(response.ItemsElementName, ItemsChoiceType2.Status);
            var isFailure = response.Error != null;
            if (statusIndex != -1 && !isFailure)
            {
                var responseStatus = Status.Parse((string)response.Items[statusIndex]);
                isFailure = !responseStatus.IsSuccessStatusCode;
            }

            var hasFailedPropStats = false;
            var failedProperties = new List<XName>();
            foreach (var propstat in response.Items.OfType<Propstat>())
            {
                var propStatIsFailure =
                    isFailure
                    || propstat.Error != null
                    || (!string.IsNullOrEmpty(propstat.Status) && !Status.Parse(propstat.Status).IsSuccessStatusCode);
                hasFailedPropStats |= propStatIsFailure;
                if (propStatIsFailure && propstat.Prop?.Any != null)
                {
                    foreach (var element in propstat.Prop.Any)
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
