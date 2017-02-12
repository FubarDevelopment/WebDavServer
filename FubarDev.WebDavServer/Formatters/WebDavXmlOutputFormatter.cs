// <copyright file="WebDavXmlOutputFormatter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using FubarDev.WebDavServer.Model;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Formatters
{
    public class WebDavXmlOutputFormatter : IWebDavOutputFormatter
    {
        private readonly ILogger<WebDavXmlOutputFormatter> _logger;
        private readonly string _namespacePrefix;

        public WebDavXmlOutputFormatter(IOptions<WebDavFormatterOptions> options, ILogger<WebDavXmlOutputFormatter> logger)
        {
            _logger = logger;
            Encoding = Encoding.UTF8;

            var contentType = options.Value.ContentType ?? "text/xml";
            ContentType = $"{contentType}; charset=\"{Encoding.WebName}\"";

            _namespacePrefix = options.Value.NamespacePrefix;
        }

        public string ContentType { get; }

        public Encoding Encoding { get; }

        public void Serialize<T>(Stream output, T data)
        {
            var writerSettings = new XmlWriterSettings();
            if (Encoding != null)
                writerSettings.Encoding = Encoding;

            var ns = new XmlSerializerNamespaces();
            if (!string.IsNullOrEmpty(_namespacePrefix))
            {
                ns.Add(_namespacePrefix, WebDavXml.Dav.NamespaceName);
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var debugOutput = new StringWriter();
                SerializerInstance<T>.Serializer.Serialize(debugOutput, data, ns);
                _logger.LogDebug(debugOutput.ToString());
            }

            using (var writer = XmlWriter.Create(output, writerSettings))
            {
                SerializerInstance<T>.Serializer.Serialize(writer, data, ns);
            }
        }

        private static class SerializerInstance<T>
        {
            public static readonly XmlSerializer Serializer = new XmlSerializer(typeof(T));
        }
    }
}
