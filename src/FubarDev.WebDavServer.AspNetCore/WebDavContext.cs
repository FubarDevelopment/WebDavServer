// <copyright file="WebDavContext.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Security.Principal;
using System.Text;

using FubarDev.WebDavServer.Utils.UAParser;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
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
            var rawPath = httpContext.GetRouteValue("path")?.ToString() ?? string.Empty;
            var path = new PathString("/" + rawPath.TrimStart('/'))
                .ToUriComponent()
                .Substring(1);
            var opt = options.Value;
            _httpContext = httpContext;
            _serviceBaseUrl = BuildServiceBaseUrl(httpContext);
            _publicBaseUrl = BuildPublicBaseUrl(httpContext, opt);
            _publicRootUrl = new Uri(PublicBaseUrl, "/");
            _serviceAbsoluteRequestUrl = BuildAbsoluteServiceUrl(httpContext);
            _serviceRootUrl = new Uri(_serviceAbsoluteRequestUrl, "/");
            _serviceRelativeRequestUrl = new Uri(_serviceAbsoluteRequestUrl.GetAbsolutePath(), UriKind.RelativeOrAbsolute);
            _publicAbsoluteRequestUrl = new Uri(_publicBaseUrl, _serviceBaseUrl.GetRelativeUrl(_serviceAbsoluteRequestUrl));
            HrefUrl = new Uri(_publicAbsoluteRequestUrl.GetAbsolutePath(), UriKind.RelativeOrAbsolute);
            _actionUrl = new Uri(path, UriKind.RelativeOrAbsolute);
            _publicRelativeRequestUrl = _publicRootUrl.GetRelativeUrl(_publicAbsoluteRequestUrl);
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
        public Uri HrefUrl { get; }

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
            var url = $"{request.Scheme}://{request.Host}{GetRawPath(httpContext)}";
            return new Uri(url);
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

        private static string GetRawPath(HttpContext httpContext)
        {
            var rawTarget = httpContext.Features.Get<IHttpRequestFeature>()?.RawTarget;
            if (string.IsNullOrEmpty(rawTarget))
                rawTarget = httpContext.Request.Path.ToString();
            return rawTarget;
        }

        private static IUAParserOutput DetectClient(HttpContext httpContext)
        {
            var userAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault();
            return Parser.GetDefault().Parse(userAgent ?? string.Empty);
        }

        private static string[] GetPathParts(string path)
        {
            return string.IsNullOrEmpty(path)
                ? Array.Empty<string>()
                : path.Split('/');
        }

        private static Uri GetControllerRelativeUrl(
            HttpContext httpContext,
            Uri serviceBaseUrl,
            Uri serviceAbsoluteRequestUrl)
        {
            var path = httpContext.GetRouteValue("path")?.ToString();
            var inputPath = serviceAbsoluteRequestUrl.GetAbsolutePath().TrimStart('/');
            if (string.IsNullOrEmpty(path))
            {
                if (!inputPath.EndsWith("/"))
                {
                    inputPath += "/";
                }

                return new Uri(inputPath, UriKind.Relative);
            }

            var serviceBasePath = serviceBaseUrl.GetAbsolutePath().TrimStart('/');
            var serviceBasePathParts = GetPathParts(serviceBasePath);
            var pathParts = GetPathParts(path);
            var inputParts = GetPathParts(inputPath);
            var resultParts = inputParts
                .Skip(serviceBasePathParts.Length)
                .Take(inputParts.Length - pathParts.Length - serviceBasePathParts.Length);
            return new Uri(
                string.Join('/', resultParts) + "/",
                UriKind.Relative);
        }
    }
}
