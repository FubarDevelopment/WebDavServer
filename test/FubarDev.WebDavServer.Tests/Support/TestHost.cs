// <copyright file="TestHost.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
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

        private IPrincipal? _user;

        public TestHost(IServiceProvider serviceProvider, Uri baseUrl, string? httpMethod)
        {
            RequestServices = serviceProvider;
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
            _requestHeaders = new Lazy<WebDavRequestHeaders>(
                () => new WebDavRequestHeaders(new HeaderDictionary(), this));
            _dispatcher = new Lazy<IWebDavDispatcher>(serviceProvider.GetRequiredService<IWebDavDispatcher>);
        }

        public TestHost(IServiceProvider serviceProvider, Uri baseUrl, IHttpContextAccessor httpContextAccessor)
        {
            RequestServices = serviceProvider;
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
                return new WebDavRequestHeaders(request.Headers, this);
            });
            _dispatcher = new Lazy<IWebDavDispatcher>(serviceProvider.GetRequiredService<IWebDavDispatcher>);
        }

        public IServiceProvider RequestServices { get; }

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

        public IPrincipal User
        {
            get => _user ??= CreateAnonymous();
            set => _user = value;
        }

        public IWebDavDispatcher Dispatcher => _dispatcher.Value;

        public string RequestMethod => _httpMethod.Value ?? throw new InvalidOperationException();

        private static IPrincipal CreateAnonymous()
        {
            return new GenericPrincipal(new GenericIdentity("anonymous"), Array.Empty<string>());
        }
    }
}
