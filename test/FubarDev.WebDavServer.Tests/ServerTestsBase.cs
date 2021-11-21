// <copyright file="ServerTestsBase.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Principal;
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
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Store;
using FubarDev.WebDavServer.Props.Store.InMemory;
using FubarDev.WebDavServer.Tests.Support;
using FubarDev.WebDavServer.Utils;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using IHttpMessageHandlerFactory = FubarDev.WebDavServer.Engines.Remote.IHttpMessageHandlerFactory;

namespace FubarDev.WebDavServer.Tests
{
    public abstract class ServerTestsBase : IDisposable
    {
        private readonly IServiceScope _serviceScope;

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

            Client = new WebDavClient(Server.CreateHandler())
            {
                BaseAddress = Server.BaseAddress,
            };

            DeadPropertyFactory = Server.Services.GetRequiredService<IDeadPropertyFactory>();

            _serviceScope = Server.Services.CreateScope();
        }

        /// <summary>
        /// Gets the WebDAV server.
        /// </summary>
        protected TestServer Server { get; }

        /// <summary>
        /// Gets the WebDAV client.
        /// </summary>
        protected WebDavClient Client { get; }

        /// <summary>
        /// Gets the factory for dead properties.
        /// </summary>
        protected IDeadPropertyFactory DeadPropertyFactory { get; }

        /// <summary>
        /// Gets the types of the controllers to be registered.
        /// </summary>
        protected virtual IEnumerable<Type> ControllerTypes { get; } = new[] { typeof(TestWebDavController) };

        /// <inheritdoc />
        public void Dispose()
        {
            Client.Dispose();
            _serviceScope.Dispose();
            Server.Dispose();
        }

        /// <summary>
        /// Gets the file system for the given user.
        /// </summary>
        /// <param name="userName">The logon name of the user.</param>
        /// <returns>The file system for the given user.</returns>
        protected IFileSystem GetFileSystem(string? userName = null)
        {
            var fsFactory = Server.Services.GetRequiredService<IFileSystemFactory>();
            var principal = new GenericPrincipal(
                new GenericIdentity(userName ?? SystemInfo.GetAnonymousUserName()),
                Array.Empty<string>());
            return fsFactory.CreateFileSystem(null, principal);
        }

        private void ConfigureServices(ServerTestsBase container, RecursiveProcessingMode processingMode, IServiceCollection services)
        {
            services
                .AddOptions()
                .AddLogging()
                .Configure<CopyHandlerOptions>(
                    opt => { opt.Mode = processingMode; })
                .Configure<MoveHandlerOptions>(
                    opt => { opt.Mode = processingMode; })
                .AddSingleton<IWebDavContextAccessor>(sp =>
                {
                    var baseUrl = container.Server.BaseAddress;
                    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                    return new TestWebDavContextAccessor(
                        baseUrl,
                        httpContextAccessor,
                        container._serviceScope.ServiceProvider);
                })
                .AddScoped(sp => sp.GetRequiredService<IWebDavContextAccessor>().WebDavContext)
                .AddScoped<IHttpMessageHandlerFactory>(_ => new TestHttpMessageHandlerFactory(container.Server))
                .AddSingleton<IFileSystemFactory, InMemoryFileSystemFactory>()
                .AddSingleton<IPropertyStoreFactory, InMemoryPropertyStoreFactory>()
                .AddSingleton<ILockManager, InMemoryLockManager>()
                .AddMvcCore()
                .ConfigureApplicationPartManager(
                    apm =>
                    {
                        apm.ApplicationParts.Clear();
                        apm.ApplicationParts.Add(new TestControllerPart(ControllerTypes));
                    })
                .AddWebDav();
        }

        private class TestWebDavContextAccessor : IWebDavContextAccessor
        {
            private readonly Uri _baseUrl;
            private readonly IHttpContextAccessor _httpContextAccessor;
            private readonly IServiceProvider _serviceProvider;
            private readonly AsyncLocal<IWebDavContext> _context = new();

            public TestWebDavContextAccessor(
                Uri baseUrl,
                IHttpContextAccessor httpContextAccessor,
                IServiceProvider serviceProvider)
            {
                _baseUrl = baseUrl;
                _httpContextAccessor = httpContextAccessor;
                _serviceProvider = serviceProvider;
            }

            public IWebDavContext WebDavContext => GetOrCreateContext();

            private IWebDavContext GetOrCreateContext()
            {
                return _context.Value ??= CreateContext();
            }

            private IWebDavContext CreateContext()
            {
                return new TestHost(_serviceProvider, _baseUrl, _httpContextAccessor);
            }
        }

        private class TestControllerPart : ApplicationPart, IApplicationPartTypeProvider
        {
            public TestControllerPart(IEnumerable<Type> types)
            {
                Types = types.Select(x => x.GetTypeInfo()).ToList();
            }

            /// <inheritdoc />
            public override string Name { get; } = "Test";

            /// <inheritdoc />
            public IEnumerable<TypeInfo> Types { get; }
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
