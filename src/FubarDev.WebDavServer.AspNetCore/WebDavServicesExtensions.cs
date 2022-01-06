// <copyright file="WebDavServicesExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.WebDavServer;
using FubarDev.WebDavServer.AspNetCore;
using FubarDev.WebDavServer.AspNetCore.Formatters.Internal;
using FubarDev.WebDavServer.BufferPools;
using FubarDev.WebDavServer.Dispatchers;
using FubarDev.WebDavServer.Engines.Remote;
using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Formatters;
using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Props;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Store;
using FubarDev.WebDavServer.Utils;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for the <see cref="IServiceCollection"/>.
    /// </summary>
    public static class WebDavServicesExtensions
    {
        /// <summary>
        /// Adds the WebDAV services that are essential to run a WebDAV server.
        /// </summary>
        /// <remarks>
        /// The user must still add the following services.
        /// <list type="bullet">
        /// <item>
        ///     <term><see cref="IFileSystemFactory"/></term>
        ///     <description>The file system factory</description>
        /// </item>
        /// <item>
        ///     <term><see cref="FubarDev.WebDavServer.Props.Store.IPropertyStoreFactory"/></term>
        ///     <description>The property store factory</description>
        /// </item>
        /// <item>
        ///     <term><see cref="ILockManager"/></term>
        ///     <description>The lock manager</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="services">The service collection to add the WebDAV services to.</param>
        /// <param name="configureOptions">WebDAV server options to configure.</param>
        /// <returns>the <paramref name="services"/>.</returns>
        public static IServiceCollection AddWebDav(
            this IServiceCollection services,
            Action<WebDavServerOptions>? configureOptions = null)
        {
            var options = new WebDavServerOptions();
            if (configureOptions != null)
            {
                configureOptions(options);
                services.Configure(configureOptions);
            }

            services.TryAddScoped<IImplicitLockFactory, DefaultImplicitLockFactory>();
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, WebDavXmlSerializerMvcOptionsSetup>());
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<IDeadPropertyFactory, DeadPropertyFactory>();
            services.TryAddScoped<IRemoteCopyTargetActionsFactory, DefaultRemoteTargetActionsFactory>();
            services.TryAddScoped<IRemoteMoveTargetActionsFactory, DefaultRemoteTargetActionsFactory>();
            services.TryAddSingleton<IHttpMessageHandlerFactory, DefaultHttpMessageHandlerFactory>();
            services.TryAddSingleton<ISystemClock, SystemClock>();
            services.TryAddSingleton<ITimeoutPolicy, DefaultTimeoutPolicy>();
            services.TryAddSingleton<IWebDavContextAccessor, WebDavContextAccessor>();
            services.TryAddSingleton<IUriComparer, DefaultUriComparer>();
            services.TryAddSingleton<IPathTraversalEngine, PathTraversalEngine>();
            services.TryAddSingleton<IMimeTypeDetector, DefaultMimeTypeDetector>();
            services.TryAddSingleton<IEntryPropertyInitializer, DefaultEntryPropertyInitializer>();
            services.TryAddSingleton<IBufferPoolFactory, ArrayPoolBufferPoolFactory>();
            services
                .AddOptions()
                .AddScoped(sp => sp.GetRequiredService<IWebDavContextAccessor>().WebDavContext)
                .AddScoped<IWebDavDispatcher, WebDavServer>()
                .AddScoped<IWebDavOutputFormatter, WebDavXmlOutputFormatter>()
                .AddSingleton<LockCleanupTask>()
                .AddScoped(sp => sp.GetRequiredService<IBufferPoolFactory>().CreatePool());
            services.Scan(
                scan => scan
                    .FromAssemblyOf<IWebDavClass>()
                    .AddClasses(classes => classes.AssignableTo<IWebDavClass1>())
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());
            if (options.EnableClass2)
            {
                services.TryAddSingleton<ILockCleanupTask, LockCleanupTask>();
                services.Scan(
                    scan => scan
                        .FromAssemblyOf<IWebDavClass>()
                        .AddClasses(classes => classes.AssignableTo<IWebDavClass2>())
                        .AsImplementedInterfaces()
                        .WithScopedLifetime());
            }

            services.Scan(
                scan => scan
                    .FromAssemblyOf<IHandler>()
                    .AddClasses(classes => classes.AssignableToAny(typeof(IHandler)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());
            services.Scan(
                scan => scan
                    .FromAssemblyOf<IDefaultDeadPropertyFactory>()
                    .AddClasses(classes => classes.AssignableToAny(typeof(IDefaultDeadPropertyFactory)))
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime());
            services.AddScoped(
                sp =>
                {
                    var factory = sp.GetRequiredService<IFileSystemFactory>();
                    var context = sp.GetRequiredService<IWebDavContext>();
                    return factory.CreateFileSystem(null, context.User);
                });
            services.AddScoped(
                sp =>
                {
                    var factory = sp.GetRequiredService<IPropertyStoreFactory>();
                    var fileSystem = sp.GetRequiredService<IFileSystem>();
                    return factory.Create(fileSystem);
                });
            return services;
        }
    }
}
