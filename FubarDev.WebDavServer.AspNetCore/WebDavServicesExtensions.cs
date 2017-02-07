// <copyright file="WebDavServicesExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer;
using FubarDev.WebDavServer.AspNetCore;
using FubarDev.WebDavServer.AspNetCore.Filters;
using FubarDev.WebDavServer.AspNetCore.Filters.Internal;
using FubarDev.WebDavServer.AspNetCore.Formatters.Internal;
using FubarDev.WebDavServer.Dispatchers;
using FubarDev.WebDavServer.Engines.Remote;
using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Formatters;
using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Props.Dead;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Scrutor;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class WebDavServicesExtensions
    {
        public static IServiceCollection AddWebDav(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, WebDavXmlSerializerMvcOptionsSetup>());
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, WebDavExceptionFilterMvcOptionsSetup>());
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<IDeadPropertyFactory, DeadPropertyFactory>();
            services.TryAddSingleton<IRemoteCopyTargetActionsFactory, DefaultRemoteTargetActionsFactory>();
            services.TryAddSingleton<IRemoteMoveTargetActionsFactory, DefaultRemoteTargetActionsFactory>();
            services.TryAddSingleton<IRemoteHttpClientFactory, DefaultRemoteHttpClientFactory>();
            services
                .AddOptions()
                .AddScoped<IWebDavDispatcher, WebDavServer>()
                .AddScoped<IWebDavHost, WebDavHost>()
                .AddSingleton<WebDavExceptionFilter>()
                .AddScoped<IWebDavOutputFormatter, WebDavXmlOutputFormatter>()
                .AddSingleton<PathTraversalEngine>()
                .AddSingleton<IDeadPropertyFactory, DeadPropertyFactory>();
            services.Scan(
                scan => scan
                    .FromAssemblyOf<IHandler>()
                    .AddClasses(classes => classes.AssignableToAny(typeof(IHandler), typeof(IWebDavClass)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());
            return services;
        }
    }
}
