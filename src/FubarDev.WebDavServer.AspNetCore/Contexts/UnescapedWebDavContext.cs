// <copyright file="UnescapedWebDavContext.cs" company="Fubar Development Junker">
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

namespace FubarDev.WebDavServer.AspNetCore.Contexts;

/// <summary>
/// The ASP.NET core specific implementation of the <see cref="IWebDavContext"/> interface.
/// </summary>
internal sealed class UnescapedWebDavContext : IWebDavContext
{
    private readonly HttpContext _httpContext;

    private readonly WebDavRequestHeaders _requestHeaders;

    private readonly Lazy<IWebDavDispatcher> _dispatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnescapedWebDavContext"/> class.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/>.</param>
    /// <param name="options">The options for the <see cref="UnescapedWebDavContext"/>.</param>
    public UnescapedWebDavContext(HttpContext httpContext, IOptions<WebDavHostOptions> options)
    {
        var rawPath = httpContext.GetRouteValue("path")?.ToString() ?? string.Empty;
        var path = new PathString("/" + rawPath.TrimStart('/'))
            .ToUriComponent()
            .Substring(1);
        var opt = options.Value;
        _httpContext = httpContext;
        ServiceBaseUrl = BuildServiceBaseUrl(httpContext);
        PublicBaseUrl = BuildPublicBaseUrl(httpContext, opt);
        PublicRootUrl = new Uri(PublicBaseUrl, "/");
        ServiceAbsoluteRequestUrl = BuildAbsoluteServiceUrl(httpContext);
        ServiceRootUrl = new Uri(ServiceAbsoluteRequestUrl, "/");
        ServiceRelativeRequestUrl = ServiceRootUrl.MakeRelativeUri(ServiceAbsoluteRequestUrl);
        PublicAbsoluteRequestUrl = new Uri(PublicBaseUrl, ServiceBaseUrl.MakeRelativeUri(ServiceAbsoluteRequestUrl));
        HrefUrl = new Uri(PublicAbsoluteRequestUrl.AbsolutePath, UriKind.RelativeOrAbsolute);
        ActionUrl = new Uri(path, UriKind.RelativeOrAbsolute);
        PublicRelativeRequestUrl = PublicRootUrl.MakeRelativeUri(PublicAbsoluteRequestUrl);
        ControllerRelativeUrl = GetControllerRelativeUrl(httpContext, ServiceBaseUrl, ServiceAbsoluteRequestUrl);
        PublicControllerUrl = new Uri(PublicBaseUrl, ControllerRelativeUrl);
        _requestHeaders = new WebDavRequestHeaders(httpContext.Request.Headers);
        DetectedClient = DetectClient(httpContext);
        User = httpContext.User;
        _dispatcher = new Lazy<IWebDavDispatcher>(_httpContext.RequestServices.GetRequiredService<IWebDavDispatcher>);
    }

    /// <inheritdoc />
    public Uri ServiceRelativeRequestUrl { get; }

    /// <inheritdoc />
    public Uri ServiceAbsoluteRequestUrl { get; }

    /// <inheritdoc />
    public Uri ServiceBaseUrl { get; }

    /// <inheritdoc />
    public Uri ServiceRootUrl { get; }

    /// <inheritdoc />
    public Uri HrefUrl { get; }

    /// <inheritdoc />
    public Uri PublicRelativeRequestUrl { get; }

    /// <inheritdoc />
    public Uri PublicAbsoluteRequestUrl { get; }

    /// <inheritdoc />
    public Uri PublicControllerUrl { get; }

    /// <inheritdoc />
    public Uri PublicBaseUrl { get; }

    /// <inheritdoc />
    public Uri PublicRootUrl { get; }

    /// <inheritdoc />
    public Uri ControllerRelativeUrl { get; }

    /// <inheritdoc />
    public Uri ActionUrl { get; }

    /// <inheritdoc />
    public IPrincipal User { get; }

    /// <inheritdoc />
    public IServiceProvider RequestServices => _httpContext.RequestServices;

    /// <inheritdoc />
    public string RequestProtocol => _httpContext.Request.Protocol;

    /// <inheritdoc />
    public IWebDavRequestHeaders RequestHeaders => _requestHeaders;

    /// <inheritdoc />
    public IUAParserOutput DetectedClient { get; }

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
            .Append(new PathString(basePath + path).ToUriComponent());
        if (request.RouteValues.TryGetValue("path", out var actionPath))
        {
            // We have an action path...
            if (string.IsNullOrEmpty(actionPath?.ToString())
                && !path.EndsWith("/"))
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
        var result = serviceBaseUrl.GetRelativeUrl(serviceControllerAbsoluteUrl);
        return result;
    }
}
