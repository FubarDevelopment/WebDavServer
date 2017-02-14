// <copyright file="TestHost.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using FubarDev.WebDavServer.Utils.UAParser;

using Microsoft.AspNetCore.Http;

namespace FubarDev.WebDavServer.Tests.Support
{
    public class TestHost : IWebDavContext
    {
        private readonly Lazy<WebDavRequestHeaders> _requestHeaders;

        public TestHost(Uri baseUrl, IHttpContextAccessor httpContextAccessor)
        {
            BaseUrl = baseUrl;
            RootUrl = new Uri(baseUrl, "/");
            RequestProtocol = baseUrl.Scheme;
            _requestHeaders = new Lazy<WebDavRequestHeaders>(() => new WebDavRequestHeaders(httpContextAccessor.HttpContext.Request.Headers.Select(x => new KeyValuePair<string, IEnumerable<string>>(x.Key, x.Value))));
        }

        public string RequestProtocol { get; }

        public Uri BaseUrl { get; }

        public Uri RootUrl { get; }

        public IUAParserOutput DetectedClient { get; } = Parser.GetDefault().Parse(string.Empty);

        public IWebDavRequestHeaders RequestHeaders => _requestHeaders.Value;
    }
}
