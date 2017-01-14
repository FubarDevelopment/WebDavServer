using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.WebDavServer.AspNetCore
{
    public static class WebDavMvcCoreBuilderExtensions
    {
        public static IMvcCoreBuilder AddWebDav(this IMvcCoreBuilder builder)
        {
            builder.Services.AddWebDav();
            return builder;
        }
    }
}
