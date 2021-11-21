// <copyright file="GenericWebDavContextAccessor.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.WebDavServer.AspNetCore;

/// <summary>
/// Generic implementation of <see cref="IWebDavContextAccessor"/>.
/// </summary>
/// <typeparam name="TContext">The WebDAV context type.</typeparam>
public class GenericWebDavContextAccessor<TContext> : IWebDavContextAccessor
    where TContext : class, IWebDavContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AsyncLocal<ActiveContext> _activeContext = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericWebDavContextAccessor{TContext}"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP Context accessor.</param>
    public GenericWebDavContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public IWebDavContext WebDavContext => GetActiveContext();

    /// <summary>
    /// Creates a new WebDAV context.
    /// </summary>
    /// <param name="httpContext">The HTTP context to create the WebDAV context for.</param>
    /// <returns>The new WebDAV context.</returns>
    protected virtual TContext BuildContext(HttpContext httpContext)
    {
        return ActivatorUtilities.CreateInstance<TContext>(
            httpContext.RequestServices,
            httpContext);
    }

    private IWebDavContext GetActiveContext()
    {
        var httpContext =
            _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No HTTP context available");

        if (_activeContext.Value == null)
        {
            var newContext = BuildContext(httpContext);
            _activeContext.Value = new ActiveContext(httpContext, newContext);
            return newContext;
        }

        var oldHttpContext = _activeContext.Value.HttpContext;
        if (oldHttpContext != httpContext)
        {
            var newContext = BuildContext(httpContext);
            _activeContext.Value = new ActiveContext(httpContext, newContext);
            return newContext;
        }

        return _activeContext.Value.WebDavContext;
    }

    private record ActiveContext(HttpContext HttpContext, TContext WebDavContext);
}
