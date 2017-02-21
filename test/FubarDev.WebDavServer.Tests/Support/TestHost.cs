// <copyright file="TestHost.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

using FubarDev.WebDavServer.Utils.UAParser;

using Microsoft.AspNetCore.Http;

namespace FubarDev.WebDavServer.Tests.Support
{
    public class TestHost : IWebDavContext
    {
        private readonly Lazy<Uri> _absoluteRequestUrl;

        private readonly Lazy<Uri> _relativeRequestUrl;

        private readonly Lazy<WebDavRequestHeaders> _requestHeaders;

        public TestHost(Uri baseUrl)
        {
            BaseUrl = baseUrl;
            RootUrl = new Uri(baseUrl, "/");
            RequestProtocol = "HTTP/1.1";
            _absoluteRequestUrl = new Lazy<Uri>(() => RootUrl);
            _relativeRequestUrl = new Lazy<Uri>(() =>
            {
                var requestUrl = RootUrl.MakeRelativeUri(baseUrl);
                if (!requestUrl.OriginalString.StartsWith("/"))
                    return new Uri("/" + requestUrl.OriginalString, UriKind.Relative);
                return requestUrl;
            });
            _requestHeaders = new Lazy<WebDavRequestHeaders>(() => new WebDavRequestHeaders(new List<KeyValuePair<string, IEnumerable<string>>>(), this));
        }

        public TestHost(Uri baseUrl, IHttpContextAccessor httpContextAccessor)
        {
            BaseUrl = baseUrl;
            RootUrl = new Uri(baseUrl, "/");
            RequestProtocol = "HTTP/1.1";
            _absoluteRequestUrl = new Lazy<Uri>(() => new Uri(RootUrl, httpContextAccessor.HttpContext.Request.Path.ToUriComponent()));
            _relativeRequestUrl = new Lazy<Uri>(() =>
            {
                var requestUrl = httpContextAccessor.HttpContext.Request.Path.ToUriComponent();
                if (!requestUrl.StartsWith("/"))
                    requestUrl = "/" + requestUrl;
                return new Uri(requestUrl, UriKind.Relative);
            });
            _requestHeaders = new Lazy<WebDavRequestHeaders>(() =>
            {
                var request = httpContextAccessor.HttpContext.Request;
                var headerItems = request.Headers.Select(x => new KeyValuePair<string, IEnumerable<string>>(x.Key, x.Value));
                return new WebDavRequestHeaders(headerItems, this);
            });
        }

        public string RequestProtocol { get; }

        public Uri RelativeRequestUrl => _relativeRequestUrl.Value;

        public Uri AbsoluteRequestUrl => _absoluteRequestUrl.Value;

        public Uri BaseUrl { get; }

        public Uri RootUrl { get; }

        public IUAParserOutput DetectedClient { get; } = Parser.GetDefault().Parse(string.Empty);

        public IWebDavRequestHeaders RequestHeaders => _requestHeaders.Value;

        public IPrincipal User { get; } = new GenericPrincipal(new GenericIdentity("anonymous"), new string[0]);
    }
}
