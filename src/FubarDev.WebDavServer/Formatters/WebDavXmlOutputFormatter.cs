// <copyright file="WebDavXmlOutputFormatter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

using FubarDev.WebDavServer.Model;

using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Formatters
{
    /// <summary>
    /// The default implementation of the <see cref="IWebDavOutputFormatter"/> interface.
    /// </summary>
    public class WebDavXmlOutputFormatter : IWebDavOutputFormatter
    {
        private static readonly Encoding _defaultEncoding = new UTF8Encoding(false);

        private readonly string _namespacePrefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavXmlOutputFormatter"/> class.
        /// </summary>
        /// <param name="options">The formatter options</param>
        public WebDavXmlOutputFormatter(IOptions<WebDavFormatterOptions> options)
        {
            Encoding = _defaultEncoding;

            var contentType = options.Value.ContentType ?? "text/xml";
            ContentType = $"{contentType}; charset=\"{Encoding.WebName}\"";

            _namespacePrefix = options.Value.NamespacePrefix;
        }

        /// <inheritdoc />
        public string ContentType { get; }

        /// <inheritdoc />
        public Encoding Encoding { get; }

        /// <inheritdoc />
        public async ValueTask SerializeAsync<T>(Stream output, T data, CancellationToken cancellationToken)
        {
            var writerSettings = new XmlWriterSettings { Encoding = Encoding };

            var ns = new XmlSerializerNamespaces();
            if (!string.IsNullOrEmpty(_namespacePrefix))
            {
                ns.Add(_namespacePrefix, WebDavXml.Dav.NamespaceName);

                if (data is XElement xelem && xelem.GetPrefixOfNamespace(WebDavXml.Dav) != _namespacePrefix)
                {
                    xelem.SetAttributeValue(XNamespace.Xmlns + _namespacePrefix, WebDavXml.Dav.NamespaceName);
                }
            }

            using var temp = new MemoryStream();
            using (var writer = XmlWriter.Create(temp, writerSettings))
            {
                SerializerInstance<T>.Serializer.Serialize(writer, data!, ns);
            }

            temp.Position = 0;
            await temp.CopyToAsync(output, 81920, cancellationToken).ConfigureAwait(false);
        }

        private static class SerializerInstance<T>
        {
            public static readonly XmlSerializer Serializer = new XmlSerializer(typeof(T));
        }
    }
}
