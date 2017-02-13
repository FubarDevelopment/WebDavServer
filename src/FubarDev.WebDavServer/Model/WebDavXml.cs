// <copyright file="WebDavXml.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml.Linq;

namespace FubarDev.WebDavServer.Model
{
    public static class WebDavXml
    {
        public static XNamespace Dav { get; } = XNamespace.Get("DAV:");
    }
}
