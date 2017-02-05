// <copyright file="MissingProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml.Linq;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Props.Filters
{
    public class MissingProperty
    {
        public MissingProperty(WebDavStatusCode statusCode, XName name)
        {
            StatusCode = statusCode;
            PropertyName = name;
        }

        public WebDavStatusCode StatusCode { get; }

        public XName PropertyName { get; }
    }
}
