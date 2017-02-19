// <copyright file="XmlFragmentExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace FubarDev.WebDavServer
{
    public static class XmlFragmentExtensions
    {
        public static IReadOnlyCollection<object> ReadXmlDocumentFragment(this string xmlStr)
        {
            var settings = new XmlReaderSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment,
                IgnoreComments = true,
            };
            using (var reader = XmlReader.Create(new StringReader(xmlStr), settings))
            {
                return reader.ReadXmlDocumentFragment();
            }
        }

        public static IReadOnlyCollection<object> ReadXmlDocumentFragment(this Stream xmlStream)
        {
            var settings = new XmlReaderSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment,
                IgnoreComments = true,
            };
            using (var reader = XmlReader.Create(xmlStream, settings))
            {
                return reader.ReadXmlDocumentFragment();
            }
        }

        public static IReadOnlyCollection<object> ReadXmlDocumentFragment(this TextReader xml)
        {
            var settings = new XmlReaderSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment,
                IgnoreComments = true,
            };
            using (var reader = XmlReader.Create(xml, settings))
            {
                return reader.ReadXmlDocumentFragment();
            }
        }

        private static IReadOnlyCollection<object> ReadXmlDocumentFragment(this XmlReader reader)
        {
            var result = new List<object>();
            while (reader.ReadState != ReadState.EndOfFile && reader.ReadState != ReadState.Closed)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    var xEl = (XElement)XNode.ReadFrom(reader);
                    result.Add(xEl);
                }
                else if (reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.CDATA)
                {
                    var text = reader.ReadContentAsString();
                    result.Add(text);
                }
                else
                {
                    throw new NotSupportedException($"Unsupported node type {reader.NodeType}");
                }
            }

            return result;
        }
    }
}
