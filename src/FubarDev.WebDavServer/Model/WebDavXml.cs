// <copyright file="WebDavXml.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model
{
    public static class WebDavXml
    {
        private const string WebDavNamespaceName = "DAV:";

        private static readonly XmlSerializer _ownerSerializer = new XmlSerializer(
            typeof(owner),
            new XmlRootAttribute("owner") { Namespace = WebDavNamespaceName, IsNullable = false });

        [NotNull]
        public static XNamespace Dav { get; } = XNamespace.Get(WebDavNamespaceName);

        [ContractAnnotation("null => null; notnull => notnull")]
        public static XElement ToXElement([CanBeNull] this owner owner)
        {
            if (owner == null)
                return null;

            using (var serialized = new StringWriter())
            {
                _ownerSerializer.Serialize(serialized, owner);
                return XElement.Parse(serialized.ToString());
            }
        }
    }
}
