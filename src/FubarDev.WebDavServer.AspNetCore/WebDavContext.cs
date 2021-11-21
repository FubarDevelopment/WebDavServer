// <copyright file="WebDavContext.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Security.Principal;
using System.Text;
using FubarDev.WebDavServer.Utils.UAParser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.AspNetCore
{
    /// <summary>
    /// The ASP.NET core specific implementation of the <see cref="IWebDavContext"/> interface.
    /// </summary>
    internal sealed class WebDavContext : IWebDavContext
    {
        private readonly HttpContext _httpContext;

        private readonly Uri _serviceAbsoluteRequestUrl;

        private readonly Uri _serviceRelativeRequestUrl;

        private readonly Uri _serviceBaseUrl;

        private readonly Uri _serviceRootUrl;

        private readonly Uri _publicRelativeRequestUrl;

        private readonly Uri _publicAbsoluteRequestUrl;

        private readonly Uri _publicBaseUrl;

        private readonly Uri _publicRootUrl;

        private readonly Uri _publicControllerUrl;

        private readonly Uri _controllerRelativeUrl;

        private readonly Uri _actionUrl;

        private readonly WebDavRequestHeaders _requestHeaders;

        private readonly IUAParserOutput _detectedClient;

        private readonly IPrincipal _principal;

        private readonly Lazy<IWebDavDispatcher> _dispatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavContext"/> class.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/>.</param>
        /// <param name="options">The options for the <see cref="WebDavContext"/>.</param>
        public WebDavContext(HttpContext httpContext, IOptions<WebDavHostOptions> options)
        {
            var opt = options.Value;
            _httpContext = httpContext;
            _serviceBaseUrl = BuildServiceBaseUrl(httpContext);
            _publicBaseUrl = BuildPublicBaseUrl(httpContext, opt);
            _publicRootUrl = new Uri(PublicBaseUrl, "/");
            _serviceAbsoluteRequestUrl = BuildAbsoluteServiceUrl(httpContext);
            _serviceRootUrl = new Uri(_serviceAbsoluteRequestUrl, "/");
            _serviceRelativeRequestUrl = ServiceRootUrl.MakeRelativeUri(_serviceAbsoluteRequestUrl);
            _publicAbsoluteRequestUrl = new Uri(_publicBaseUrl, _serviceBaseUrl.MakeRelativeUri(_serviceAbsoluteRequestUrl));
            _actionUrl = new Uri(Uri.EscapeUriString(httpContext.GetRouteValue("path")?.ToString() ?? string.Empty), UriKind.RelativeOrAbsolute);
            _publicRelativeRequestUrl = _publicRootUrl.MakeRelativeUri(_publicAbsoluteRequestUrl);
            _controllerRelativeUrl = GetControllerRelativeUrl(httpContext, _serviceBaseUrl, _serviceAbsoluteRequestUrl);
            _publicControllerUrl = new Uri(_publicBaseUrl, _controllerRelativeUrl);
            _requestHeaders = new WebDavRequestHeaders(httpContext.Request.Headers, this);
            _detectedClient = DetectClient(httpContext);
            _principal = httpContext.User;
            _dispatcher = new Lazy<IWebDavDispatcher>(_httpContext.RequestServices.GetRequiredService<IWebDavDispatcher>);
        }

        /// <inheritdoc />
        public Uri ServiceRelativeRequestUrl => _serviceRelativeRequestUrl;

        /// <inheritdoc />
        public Uri ServiceAbsoluteRequestUrl => _serviceAbsoluteRequestUrl;

        /// <inheritdoc />
        public Uri ServiceBaseUrl => _serviceBaseUrl;

        /// <inheritdoc />
        public Uri ServiceRootUrl => _serviceRootUrl;

        /// <inheritdoc />
        public Uri PublicRelativeRequestUrl => _publicRelativeRequestUrl;

        /// <inheritdoc />
        public Uri PublicAbsoluteRequestUrl => _publicAbsoluteRequestUrl;

        /// <inheritdoc />
        public Uri PublicControllerUrl => _publicControllerUrl;

        /// <inheritdoc />
        public Uri PublicBaseUrl => _publicBaseUrl;

        /// <inheritdoc />
        public Uri PublicRootUrl => _publicRootUrl;

        /// <inheritdoc />
        public Uri ControllerRelativeUrl => _controllerRelativeUrl;

        /// <inheritdoc />
        public Uri ActionUrl => _actionUrl;

        /// <inheritdoc />
        public IPrincipal User => _principal;

        /// <inheritdoc />
        public IServiceProvider RequestServices => _httpContext.RequestServices;

        /// <inheritdoc />
        public string RequestProtocol => _httpContext.Request.Protocol;

        /// <inheritdoc />
        public IWebDavRequestHeaders RequestHeaders => _requestHeaders;

        /// <inheritdoc />
        public IUAParserOutput DetectedClient => _detectedClient;

        /// <inheritdoc />
        public IWebDavDispatcher Dispatcher => _dispatcher.Value;

        /// <inheritdoc />
        public string RequestMethod => _httpContext.Request.Method;

        private static Uri BuildAbsoluteServiceUrl(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var result = new StringBuilder();
            var basePath = request.PathBase.Value ?? string.Empty;
            var path = request.Path.Value ?? string.Empty;
            if (!basePath.EndsWith("/") && !path.StartsWith("/"))
            {
                basePath += "/";
            }

            result.Append(request.Scheme).Append("://").Append(request.Host)
                .Append(basePath)
                .Append(Uri.EscapeUriString(path));
            if (request.RouteValues.TryGetValue("path", out var actionPath))
            {
                // We have an action path...
                if (string.IsNullOrEmpty(actionPath?.ToString()))
                {
                    // The path for the action is empty, which means that
                    // the WebDAV client queried the root entry of the file
                    // system.
                    result.Append("/");
                }
            }

            var resultUrl = new Uri(result.ToString());
            return resultUrl;
        }

        private static Uri BuildPublicBaseUrl(HttpContext httpContext, WebDavHostOptions options)
        {
            if (options.BaseUrl == null)
            {
                return BuildServiceBaseUrl(httpContext);
            }

            var result = new StringBuilder();
            result.Append(options.BaseUrl);

            var resultUrl = result.ToString();
            if (!resultUrl.EndsWith("/", StringComparison.Ordinal))
            {
                resultUrl += "/";
            }

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
            {
                resultUrl += "/";
            }

            return new Uri(resultUrl);
        }

        private static IUAParserOutput DetectClient(HttpContext httpContext)
        {
            var userAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault();
            return Parser.GetDefault().Parse(userAgent ?? string.Empty);
        }

        private static Uri GetControllerRelativeUrl(
            HttpContext httpContext,
            Uri serviceBaseUrl,
            Uri serviceAbsoluteRequestUrl)
        {
            var path = httpContext.GetRouteValue("path")?.ToString();
            var input = Uri.UnescapeDataString(serviceAbsoluteRequestUrl.ToString());
            string remaining = input;
            if (path != null)
            {
                int pathIndex = input.LastIndexOf(path, StringComparison.Ordinal);
                if (pathIndex != -1)
                {
                    remaining = input.Substring(0, pathIndex);
                }
            }

            var serviceControllerAbsoluteUrl = new Uri(remaining);
            var result = serviceBaseUrl.MakeRelativeUri(serviceControllerAbsoluteUrl);
            return result;
        }
    }
}
