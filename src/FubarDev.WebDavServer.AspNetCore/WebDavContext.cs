// <copyright file="WebDavContext.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;

using FubarDev.WebDavServer.Utils.UAParser;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.AspNetCore
{
    /// <summary>
    /// The ASP.NET core specific implementation of the <see cref="IWebDavContext"/> interface
    /// </summary>
    public class WebDavContext : IWebDavContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly Lazy<Uri> _absoluteRequestUrl;

        private readonly Lazy<Uri> _relativeRequestUrl;

        private readonly Lazy<Uri> _baseUrl;

        private readonly Lazy<Uri> _rootUrl;

        private readonly Lazy<WebDavRequestHeaders> _requestHeaders;

        private readonly Lazy<IUAParserOutput> _detectedClient;

        private readonly Lazy<IPrincipal> _principal;

        private readonly Lazy<IWebDavDispatcher> _dispatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavContext"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to get the <see cref="IWebDavDispatcher"/> with</param>
        /// <param name="httpContextAccessor">The <see cref="HttpContext"/> accessor</param>
        /// <param name="options">The options for the <see cref="WebDavContext"/></param>
        public WebDavContext(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor, IOptions<WebDavHostOptions> options)
        {
            var opt = options?.Value ?? new WebDavHostOptions();
            _httpContextAccessor = httpContextAccessor;
            _baseUrl = new Lazy<Uri>(() => BuildBaseUrl(httpContextAccessor.HttpContext, opt));
            _rootUrl = new Lazy<Uri>(() => new Uri(_baseUrl.Value, "/"));
            _absoluteRequestUrl = new Lazy<Uri>(() => new Uri(_rootUrl.Value, httpContextAccessor.HttpContext.Request.Path.ToUriComponent()));
            _relativeRequestUrl = new Lazy<Uri>(() =>
            {
                var requestPath = httpContextAccessor.HttpContext.Request.Path.ToUriComponent();
                if (!requestPath.StartsWith("/"))
                    return new Uri("/" + requestPath, UriKind.Relative);
                return new Uri(requestPath, UriKind.Relative);
            });
            _requestHeaders = new Lazy<WebDavRequestHeaders>(() =>
            {
                var request = httpContextAccessor.HttpContext.Request;
                var headerItems = request.Headers.Select(x => new KeyValuePair<string, IEnumerable<string>>(x.Key, x.Value));
                return new WebDavRequestHeaders(headerItems, this);
            });
            _detectedClient = new Lazy<IUAParserOutput>(() => DetectClient(httpContextAccessor.HttpContext));
            _principal = new Lazy<IPrincipal>(() => httpContextAccessor.HttpContext.User);
            _dispatcher = new Lazy<IWebDavDispatcher>(serviceProvider.GetRequiredService<IWebDavDispatcher>);
        }

        /// <inheritdoc />
        public Uri RelativeRequestUrl => _relativeRequestUrl.Value;

        /// <inheritdoc />
        public Uri AbsoluteRequestUrl => _absoluteRequestUrl.Value;

        /// <inheritdoc />
        public Uri BaseUrl => _baseUrl.Value;

        /// <inheritdoc />
        public Uri RootUrl => _rootUrl.Value;

        /// <inheritdoc />
        public IPrincipal User => _principal.Value;

        /// <inheritdoc />
        public string RequestProtocol => _httpContextAccessor.HttpContext.Request.Protocol;

        /// <inheritdoc />
        public IWebDavRequestHeaders RequestHeaders => _requestHeaders.Value;

        /// <inheritdoc />
        public IUAParserOutput DetectedClient => _detectedClient.Value;

        /// <inheritdoc />
        public IWebDavDispatcher Dispatcher => _dispatcher.Value;

        private static Uri BuildBaseUrl(HttpContext httpContext, WebDavHostOptions options)
        {
            var result = new StringBuilder();
            if (options.BaseUrl != null)
            {
                result.Append(options.BaseUrl);
                if (!options.BaseUrl.EndsWith("/", StringComparison.Ordinal))
                    result.Append("/");
            }
            else
            {
                var request = httpContext.Request;
                var pathBase = request.PathBase.ToString();
                result.Append(request.Scheme).Append("://").Append(request.Host).Append(pathBase);
                if (!pathBase.EndsWith("/", StringComparison.Ordinal))
                    result.Append("/");
            }

            return new Uri(result.ToString());
        }

        private static IUAParserOutput DetectClient(HttpContext httpContext)
        {
            var userAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault();
            return Parser.GetDefault().Parse(userAgent ?? string.Empty);
        }
    }
}
