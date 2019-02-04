// <copyright file="WebDavXmlOutputFormatter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Formatters
{
    /// <summary>
    /// The default implementation of the <see cref="IWebDavOutputFormatter"/> interface.
    /// </summary>
    public class WebDavXmlOutputFormatter : IWebDavOutputFormatter
    {
        [NotNull]
        private static readonly Encoding _defaultEncoding = new UTF8Encoding(false);

        [NotNull]
        private readonly string _namespacePrefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavXmlOutputFormatter"/> class.
        /// </summary>
        /// <param name="options">The formatter options</param>
        public WebDavXmlOutputFormatter([NotNull] IOptions<WebDavFormatterOptions> options)
        {
            Encoding = _defaultEncoding;

            var contentType = options.Value.ContentType ?? "text/xml";
            ContentType = $"{contentType}; charset=\"{Encoding.WebName}\"";

            _namespacePrefix = options.Value.NamespacePrefix;
        }

        /// <inheritdoc />
        [NotNull]
        public string ContentType { get; }

        /// <inheritdoc />
        [NotNull]
        public Encoding Encoding { get; }

        /// <inheritdoc />
        public void Serialize<T>(Stream output, T data)
        {
            var writerSettings = new XmlWriterSettings { Encoding = Encoding };

            var ns = new XmlSerializerNamespaces();
            if (!string.IsNullOrEmpty(_namespacePrefix))
            {
                ns.Add(_namespacePrefix, WebDavXml.Dav.NamespaceName);

                var xelem = data as XElement;
                if (xelem != null && xelem.GetPrefixOfNamespace(WebDavXml.Dav) != _namespacePrefix)
                {
                    xelem.SetAttributeValue(XNamespace.Xmlns + _namespacePrefix, WebDavXml.Dav.NamespaceName);
                }
            }

            using (var writer = XmlWriter.Create(output, writerSettings))
            {
                SerializerInstance<T>.Serializer.Serialize(writer, data, ns);
            }
        }

        private static class SerializerInstance<T>
        {
            [NotNull]
            public static readonly XmlSerializer Serializer = new XmlSerializer(typeof(T));
        }
    }
}
