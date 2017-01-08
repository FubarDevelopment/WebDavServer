using FubarDev.WebDavServer.AspNetCore.Filters;
using FubarDev.WebDavServer.AspNetCore.Filters.Internal;
using FubarDev.WebDavServer.AspNetCore.Formatters.Internal;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.AspNetCore
{
    internal static class WebDavServicesExtensions
    {
        public static IServiceCollection AddWebDavServices(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, WebDavXmlSerializerMvcOptionsSetup>());
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, WebDavExceptionFilterMvcOptionsSetup>());
            services.AddSingleton<WebDavExceptionFilter>();
            return services;
        }
    }
}
