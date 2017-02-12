// <copyright file="LockTestsBase.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace FubarDev.WebDavServer.Tests.Locking
{
    public abstract class LockTestsBase : IClassFixture<LockServices>, IDisposable
    {
        private readonly IServiceScope _scope;

        protected LockTestsBase(LockServices services)
        {
            var scopeFactory = services.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
            _scope = scopeFactory.CreateScope();
        }

        protected IServiceProvider ServiceProvider => _scope.ServiceProvider;

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}
