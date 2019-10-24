// <copyright file="TestHost.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

using FubarDev.WebDavServer.Utils.UAParser;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.WebDavServer.Tests.Support
{
    public class TestHost : IWebDavContext
    {
        private readonly Lazy<Uri> _absoluteRequestUrl;

        private readonly Lazy<Uri> _relativeRequestUrl;

        private readonly Lazy<WebDavRequestHeaders> _requestHeaders;

        private readonly Lazy<IWebDavDispatcher> _dispatcher;

        private readonly Lazy<string?> _httpMethod;

        public TestHost(IServiceProvider serviceProvider, Uri baseUrl, string? httpMethod)
        {
            _httpMethod = new Lazy<string?>(() => httpMethod);
            PublicBaseUrl = baseUrl;
            PublicRootUrl = new Uri(baseUrl, "/");
            RequestProtocol = "HTTP/1.1";
            _absoluteRequestUrl = new Lazy<Uri>(() => PublicRootUrl);
            _relativeRequestUrl = new Lazy<Uri>(() =>
            {
                var requestUrl = PublicRootUrl.MakeRelativeUri(baseUrl);
                if (!requestUrl.OriginalString.StartsWith("/"))
                    return new Uri("/" + requestUrl.OriginalString, UriKind.Relative);
                return requestUrl;
            });
            _requestHeaders = new Lazy<WebDavRequestHeaders>(() => new WebDavRequestHeaders(new List<KeyValuePair<string, IEnumerable<string>>>(), this));
            _dispatcher = new Lazy<IWebDavDispatcher>(serviceProvider.GetRequiredService<IWebDavDispatcher>);
        }

        public TestHost(IServiceProvider serviceProvider, Uri baseUrl, IHttpContextAccessor httpContextAccessor)
        {
            _httpMethod = new Lazy<string?>(() => httpContextAccessor.HttpContext.Request.Method);
            PublicBaseUrl = baseUrl;
            PublicRootUrl = new Uri(baseUrl, "/");
            RequestProtocol = "HTTP/1.1";
            _absoluteRequestUrl = new Lazy<Uri>(() => new Uri(PublicRootUrl, httpContextAccessor.HttpContext.Request.Path.ToUriComponent()));
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
            _dispatcher = new Lazy<IWebDavDispatcher>(serviceProvider.GetRequiredService<IWebDavDispatcher>);
        }

        public string RequestProtocol { get; }

        public Uri ServiceRelativeRequestUrl => PublicRelativeRequestUrl;

        public Uri ServiceAbsoluteRequestUrl => PublicAbsoluteRequestUrl;

        public Uri ServiceBaseUrl => PublicBaseUrl;

        public Uri ServiceRootUrl => PublicRootUrl;

        public Uri ControllerRelativeUrl { get; } = new Uri(string.Empty, UriKind.RelativeOrAbsolute);

        public Uri ActionUrl => PublicRelativeRequestUrl;

        public Uri PublicRelativeRequestUrl => _relativeRequestUrl.Value;

        public Uri PublicAbsoluteRequestUrl => _absoluteRequestUrl.Value;

        public Uri PublicControllerUrl => PublicBaseUrl;

        public Uri PublicBaseUrl { get; }

        public Uri PublicRootUrl { get; }

        public IUAParserOutput DetectedClient { get; } = Parser.GetDefault().Parse(string.Empty);

        public IWebDavRequestHeaders RequestHeaders => _requestHeaders.Value;

        public IPrincipal User { get; } = new GenericPrincipal(new GenericIdentity("anonymous"), new string[0]);

        public IWebDavDispatcher Dispatcher => _dispatcher.Value;

        public string RequestMethod => _httpMethod.Value ?? throw new InvalidOperationException();
    }
}
