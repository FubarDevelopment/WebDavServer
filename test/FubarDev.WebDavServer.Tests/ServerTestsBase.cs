// <copyright file="ServerTestsBase.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using DecaTec.WebDav;

using FubarDev.WebDavServer.AspNetCore;
using FubarDev.WebDavServer.AspNetCore.Logging;
using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.InMemory;
using FubarDev.WebDavServer.Handlers.Impl;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Locking.InMemory;
using FubarDev.WebDavServer.Props.Store;
using FubarDev.WebDavServer.Props.Store.InMemory;
using FubarDev.WebDavServer.Tests.Support;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using IHttpMessageHandlerFactory = FubarDev.WebDavServer.Engines.Remote.IHttpMessageHandlerFactory;

namespace FubarDev.WebDavServer.Tests
{
    public abstract class ServerTestsBase : IDisposable
    {
        private readonly IServiceScope _scope;

        protected ServerTestsBase()
            : this(RecursiveProcessingMode.PreferFastest)
        {
        }

        protected ServerTestsBase(RecursiveProcessingMode processingMode)
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(sc => ConfigureServices(this, processingMode, sc))
                .ConfigureLogging(
                    loggingBuilder =>
                    {
                        loggingBuilder.AddDebug();
                        loggingBuilder.AddFilter(
                            "FubarDev.WebDavServer.AspNetCore.WebDavIndirectResult",
                            LogLevel.Information);
                        loggingBuilder.SetMinimumLevel(LogLevel.Debug);
                    })
                .UseStartup<TestStartup>();

            Server = new TestServer(builder);
            _scope = Server.Host.Services.CreateScope();

            Client = new WebDavClient(Server.CreateHandler())
            {
                BaseAddress = Server.BaseAddress,
            };

            FileSystem = _scope.ServiceProvider.GetRequiredService<IFileSystem>();
        }

        protected IFileSystem FileSystem { get; }

        protected TestServer Server { get; }

        protected WebDavClient Client { get; }

        protected IServiceProvider ServiceProvider => _scope.ServiceProvider;

        public void Dispose()
        {
            Server.Dispose();
            Client.Dispose();
            _scope.Dispose();
        }

        private void ConfigureServices(ServerTestsBase container, RecursiveProcessingMode processingMode, IServiceCollection services)
        {
            IFileSystemFactory? fileSystemFactory = null;
            IPropertyStoreFactory? propertyStoreFactory = null;
            services
                .AddOptions()
                .AddLogging()
                .Configure<CopyHandlerOptions>(
                    opt =>
                    {
                        opt.Mode = processingMode;
                    })
                .Configure<MoveHandlerOptions>(
                    opt =>
                    {
                        opt.Mode = processingMode;
                    })
                .AddScoped<IWebDavContext>(sp => new TestHost(sp, container.Server.BaseAddress, sp.GetRequiredService<IHttpContextAccessor>()))
                .AddScoped<IHttpMessageHandlerFactory>(sp => new TestHttpMessageHandlerFactory(container.Server))
                .AddScoped(sp => fileSystemFactory ?? (fileSystemFactory = ActivatorUtilities.CreateInstance<InMemoryFileSystemFactory>(sp)))
                .AddScoped(sp => propertyStoreFactory ?? (propertyStoreFactory = ActivatorUtilities.CreateInstance<InMemoryPropertyStoreFactory>(sp)))
                .AddSingleton<ILockManager, InMemoryLockManager>()
                .AddMvcCore()
                .AddApplicationPart(typeof(TestWebDavController).GetTypeInfo().Assembly)
                .AddWebDav();
        }

        private class TestStartup
        {
            public IServiceProvider ConfigureServices(IServiceCollection services)
            {
                return services.BuildServiceProvider(true);
            }

            public void Configure(IApplicationBuilder app)
            {
                app.UseMiddleware<RequestLogMiddleware>();
                app.UseRouting();
                app.UseEndpoints(
                    routes => { routes.MapControllers(); });
            }
        }

        private class TestHttpMessageHandlerFactory : IHttpMessageHandlerFactory
        {
            private readonly TestServer _server;

            public TestHttpMessageHandlerFactory(TestServer server)
            {
                _server = server;
            }

            public Task<HttpMessageHandler> CreateAsync(Uri baseUrl, CancellationToken cancellationToken)
            {
                return Task.FromResult(_server.CreateHandler());
            }
        }
    }
}
