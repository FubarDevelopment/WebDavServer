// <copyright file="TestWebDavContextAccessor.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.WebDavServer.Tests.Support;

public class TestWebDavContextAccessor : IWebDavContextAccessor, IDisposable
{
    private readonly IServiceScope _serviceScope;

    public TestWebDavContextAccessor(IServiceProvider serviceProvider)
    {
        _serviceScope = serviceProvider.CreateScope();
        WebDavContext = new TestHost(_serviceScope.ServiceProvider, new Uri("http://localhost/"), (string?)null);
    }

    public IWebDavContext WebDavContext { get; }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}
