using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace DecaTec.WebDav
{
    /// <summary>
    /// Helper functions for WebDAV.
    /// </summary>
    public static partial class WebDavHelper
    {
        /// <summary>
        /// Gets a UTF-8 encoded string by serializing the object specified.
        /// </summary>
        /// <param name="xmlSerializer">The <see cref="XmlSerializer"/> to use.</param>
        /// <param name="objectToSerialize">The object which should be serialized.</param>
        /// <returns>A UTF-8 encoded string containing the serializes object.</returns>
        public static string GetUtf8EncodedXmlWebDavRequestString(XmlSerializer xmlSerializer, object objectToSerialize)
        {
            var xmlNamespace = new KeyValuePair<string, string>[1];
            xmlNamespace[0] = new KeyValuePair<string, string>("D", WebDavConstants.DAV);
            return GetUtf8EncodedXmlWebDavRequestString(xmlSerializer, objectToSerialize, xmlNamespace);
        }

        /// <summary>
        /// Gets a UTF-8 encoded string by serializing the object specified.
        /// </summary>
        /// <param name="xmlSerializer">The <see cref="XmlSerializer"/> to use.</param>
        /// <param name="objectToSerialize">The object which should be serialized.</param>
        /// <param name="xmlNamespaces">The namespaces to include.</param>
        /// <returns>A UTF-8 encoded string containing the serializes object.</returns>
        public static string GetUtf8EncodedXmlWebDavRequestString(XmlSerializer xmlSerializer, object objectToSerialize, KeyValuePair<string, string>[] xmlNamespaces)
        {
            try
            {
                using (var mStream = new MemoryStream())
                {
                    var xnameSpace = new XmlSerializerNamespaces();

                    foreach (var kvp in xmlNamespaces)
                    {
                        xnameSpace.Add(kvp.Key, kvp.Value);
                    }

                    // Always add WebDAV namespace.
                    xnameSpace.Add("D", WebDavConstants.DAV);

                    var utf8Encoding = new UTF8Encoding();
                    var xmlWriter = XmlWriter.Create(mStream, new XmlWriterSettings() { Encoding = utf8Encoding });
                    xmlSerializer.Serialize(xmlWriter, objectToSerialize, xnameSpace);
                    byte[] bArr = mStream.ToArray();
                    return utf8Encoding.GetString(bArr, 0, bArr.Length);
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
