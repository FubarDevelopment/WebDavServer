using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines.Remote
{
    public abstract class RemoteHttpClientTargetActions : RemoteTargetActions, IDisposable
    {
        private static readonly XmlSerializer _errorSerializer = new XmlSerializer(typeof(Error));
        private static readonly XmlSerializer _multiStatusSerializer = new XmlSerializer(typeof(Multistatus));
        private readonly HttpClient _client;

        protected RemoteHttpClientTargetActions(IRemoteHttpClientFactory remoteHttpClientFactory)
        {
            _client = remoteHttpClientFactory.Create();
        }

        public override RecursiveTargetBehaviour ExistingTargetBehaviour { get; } = RecursiveTargetBehaviour.Overwrite;

        public override Task<IReadOnlyCollection<XName>> SetPropertiesAsync(RemoteCollectionTarget target, IEnumerable<IUntypedWriteableProperty> properties, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public override Task<IReadOnlyCollection<XName>> SetPropertiesAsync(RemoteDocumentTarget target, IEnumerable<IUntypedWriteableProperty> properties, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public override Task<RemoteCollectionTarget> CreateCollectionAsync(RemoteCollectionTarget targetCollection, string name, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public override Task<ITarget> GetAsync(string name, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public override async Task DeleteAsync(RemoteCollectionTarget target, CancellationToken cancellationToken)
        {
            using (var response = await _client.DeleteAsync(target.DestinationUrl, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
            }
        }

        public override async Task DeleteAsync(RemoteDocumentTarget target, CancellationToken cancellationToken)
        {
            using (var response = await _client.DeleteAsync(target.DestinationUrl, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
            }
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
                    }
                };
            }

            var errorName = WebDavXml.Dav + "error";
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
                    }
                };
            }

            var result = (Multistatus)_multiStatusSerializer.Deserialize(document.CreateReader());
            return result;
        }

        [NotNull, ItemCanBeNull]
        protected async Task<XDocument> ReadResponse([NotNull] HttpResponseMessage responseMessage, CancellationToken cancellationToken)
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

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
