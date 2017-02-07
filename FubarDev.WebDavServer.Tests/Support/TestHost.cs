// <copyright file="TestHost.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.WebDavServer.Tests.Support
{
    public class TestHost : IWebDavHost
    {
        public TestHost()
            : this(new Uri("http://localhost/"))
        {
        }

        public TestHost(Uri baseUrl)
        {
            BaseUrl = baseUrl;
            RequestProtocol = baseUrl.Scheme;
        }

        public string RequestProtocol { get; }

        public Uri BaseUrl { get; }

        public DetectedClient DetectedClient { get; } = DetectedClient.Any;
    }
}
