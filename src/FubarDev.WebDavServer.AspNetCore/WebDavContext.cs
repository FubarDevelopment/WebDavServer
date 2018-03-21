// <copyright file="WebDavContext.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;

using FubarDev.WebDavServer.Utils.UAParser;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.AspNetCore
{
    /// <summary>
    /// The ASP.NET core specific implementation of the <see cref="IWebDavContext"/> interface
    /// </summary>
    public sealed class WebDavContext : IWebDavContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly Lazy<Uri> _serviceAbsoluteRequestUrl;

        private readonly Lazy<Uri> _serviceRelativeRequestUrl;

        private readonly Lazy<Uri> _serviceBaseUrl;

        private readonly Lazy<Uri> _serviceRootUrl;

        private readonly Lazy<Uri> _publicRelativeRequestUrl;

        private readonly Lazy<Uri> _publicAbsoluteRequestUrl;

        private readonly Lazy<Uri> _publicBaseUrl;

        private readonly Lazy<Uri> _publicRootUrl;

        private readonly Lazy<Uri> _publicControllerUrl;

        private readonly Lazy<Uri> _controllerRelativeUrl;

        private readonly Lazy<Uri> _actionUrl;

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
            _serviceBaseUrl = new Lazy<Uri>(() => BuildServiceBaseUrl(httpContextAccessor.HttpContext));
            _publicBaseUrl = new Lazy<Uri>(() => BuildPublicBaseUrl(httpContextAccessor.HttpContext, opt));
            _publicRootUrl = new Lazy<Uri>(() => new Uri(PublicBaseUrl, "/"));
            _serviceAbsoluteRequestUrl = new Lazy<Uri>(() => BuildAbsoluteServiceUrl(httpContextAccessor.HttpContext));
            _serviceRootUrl = new Lazy<Uri>(() => new Uri(ServiceAbsoluteRequestUrl, "/"));
            _serviceRelativeRequestUrl = new Lazy<Uri>(() => ServiceRootUrl.MakeRelativeUri(ServiceAbsoluteRequestUrl));
            _publicAbsoluteRequestUrl = new Lazy<Uri>(() => new Uri(PublicBaseUrl, ServiceBaseUrl.MakeRelativeUri(ServiceAbsoluteRequestUrl)));
            _actionUrl = new Lazy<Uri>(() => new Uri(httpContextAccessor.HttpContext.GetRouteValue("path").ToString(), UriKind.RelativeOrAbsolute));
            _publicRelativeRequestUrl = new Lazy<Uri>(() => new Uri(PublicBaseUrl, ActionUrl));
            _publicControllerUrl = new Lazy<Uri>(() => new Uri(PublicBaseUrl, ControllerRelativeUrl));
            _controllerRelativeUrl = new Lazy<Uri>(
                () =>
                {
                    var path = httpContextAccessor.HttpContext.GetRouteValue("path")?.ToString();
                    var input = ServiceAbsoluteRequestUrl.ToString();
                    string remaining;
                    if (path != null)
                    {
                        var pattern = string.Format("{0}$", Regex.Escape(path));
                        remaining = Regex.Replace(input, pattern, string.Empty);
                    }
                    else
                    {
                        remaining = input;
                    }

                    var serviceControllerAbsoluteUrl = new Uri(remaining);
                    var result = ServiceBaseUrl.MakeRelativeUri(serviceControllerAbsoluteUrl);
                    return result;
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
        public Uri ServiceRelativeRequestUrl => _serviceRelativeRequestUrl.Value;

        /// <inheritdoc />
        public Uri ServiceAbsoluteRequestUrl => _serviceAbsoluteRequestUrl.Value;

        /// <inheritdoc />
        public Uri ServiceBaseUrl => _serviceBaseUrl.Value;

        /// <inheritdoc />
        public Uri ServiceRootUrl => _serviceRootUrl.Value;

        /// <inheritdoc />
        public Uri PublicRelativeRequestUrl => _publicRelativeRequestUrl.Value;

        /// <inheritdoc />
        public Uri PublicAbsoluteRequestUrl => _publicAbsoluteRequestUrl.Value;

        /// <inheritdoc />
        public Uri PublicControllerUrl => _publicControllerUrl.Value;

        /// <inheritdoc />
        public Uri PublicBaseUrl => _publicBaseUrl.Value;

        /// <inheritdoc />
        public Uri PublicRootUrl => _publicRootUrl.Value;

        /// <inheritdoc />
        public Uri ControllerRelativeUrl => _controllerRelativeUrl.Value;

        /// <inheritdoc />
        public Uri ActionUrl => _actionUrl.Value;

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

        private static Uri BuildAbsoluteServiceUrl(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var result = new StringBuilder();
            var basePath = request.PathBase.ToString();
            var path = request.Path.ToString();
            if (!basePath.EndsWith("/") && !path.StartsWith("/"))
                basePath += "/";
            result.Append(request.Scheme).Append("://").Append(request.Host)
                .Append(basePath)
                .Append(path);

            return new Uri(result.ToString());
        }

        private static Uri BuildPublicBaseUrl(HttpContext httpContext, WebDavHostOptions options)
        {
            if (options.BaseUrl == null)
                return BuildServiceBaseUrl(httpContext);

            var result = new StringBuilder();
            result.Append(options.BaseUrl);

            var resultUrl = result.ToString();
            if (!resultUrl.EndsWith("/", StringComparison.Ordinal))
                resultUrl += "/";

            return new Uri(resultUrl);
        }

        private static Uri BuildServiceBaseUrl(HttpContext httpContext)
        {
            var result = new StringBuilder();
            var request = httpContext.Request;
            result.Append(request.Scheme).Append("://").Append(request.Host)
                .Append(request.PathBase);

            var resultUrl = result.ToString();
            if (!resultUrl.EndsWith("/", StringComparison.Ordinal))
                resultUrl += "/";

            return new Uri(resultUrl);
        }

        private static IUAParserOutput DetectClient(HttpContext httpContext)
        {
            var userAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault();
            return Parser.GetDefault().Parse(userAgent ?? string.Empty);
        }
    }
}
