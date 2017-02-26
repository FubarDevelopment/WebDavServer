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
    /// <summary>
    /// Extension methods to handle XML document fragments
    /// </summary>
    public static class XmlFragmentExtensions
    {
        /// <summary>
        /// Reads the XML document fragment from a <see cref="string"/>
        /// </summary>
        /// <param name="xmlStr">XML string to read the document fragment from</param>
        /// <returns>The object collection</returns>
        /// <remarks>
        /// The returned collection may contain either an <see cref="XElement"/> or a <see cref="string"/>.
        /// </remarks>
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

        /// <summary>
        /// Reads the XML document fragment from a <see cref="Stream"/>
        /// </summary>
        /// <param name="xmlStream">The <see cref="Stream"/> to read the XML document fragment from</param>
        /// <returns>The object collection</returns>
        /// <remarks>
        /// The returned collection may contain either an <see cref="XElement"/> or a <see cref="string"/>.
        /// </remarks>
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

        /// <summary>
        /// Reads the XML document fragment from a <see cref="TextReader"/>
        /// </summary>
        /// <param name="xml">The <see cref="TextReader"/> to read the document fragment from</param>
        /// <returns>The object collection</returns>
        /// <remarks>
        /// The returned collection may contain either an <see cref="XElement"/> or a <see cref="string"/>.
        /// </remarks>
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

        /// <summary>
        /// Reads data from a XML <paramref name="reader"/> and puts all found elements into a collection of <see cref="object"/>
        /// </summary>
        /// <param name="reader">The reader to read from</param>
        /// <returns>The object collection</returns>
        /// <remarks>
        /// The returned collection may contain either an <see cref="XElement"/> or a <see cref="string"/>.
        /// </remarks>
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
