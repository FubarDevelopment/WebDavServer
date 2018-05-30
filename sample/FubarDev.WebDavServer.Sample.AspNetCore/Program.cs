using System;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Sample.AspNetCore
{
    public class Program
    {
        public static bool IsKestrel { get; private set; }

        public static bool DisableBasicAuth { get; private set; }

        public static bool IsWindows => Environment.OSVersion.Platform == PlatformID.Win32NT;

        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        private static IWebHost BuildWebHost(string[] args)
        {
            var tempHost = BuildWebHost(args, whb => whb);
            var config = tempHost.Services.GetRequiredService<IConfiguration>();

            DisableBasicAuth = config.GetValue<bool>("disable-basic-auth");

            return BuildWebHost(
                args,
                builder => ConfigureHosting(builder, config));
        }

        private static IWebHost BuildWebHost(string[] args, Func<IWebHostBuilder, IWebHostBuilder> customConfig) =>
            customConfig(WebHost.CreateDefaultBuilder(args))
                .ConfigureLogging((ctxt, b) => { b.AddExceptionDemystifyer(); })
                .UseStartup<Startup>()
                .Build();

        private static IWebHostBuilder ConfigureHosting(IWebHostBuilder builder, IConfiguration configuration)
        {
            var forceKestrelUse = configuration.GetValue<bool>("use-kestrel");
            if (!forceKestrelUse && IsWindows)
            {
                builder = builder
                    .UseHttpSys(
                        opt =>
                        {
                            opt.Authentication.Schemes = AuthenticationSchemes.NTLM;
                            opt.Authentication.AllowAnonymous = true;
                        });
            }
            else
            {
                IsKestrel = true;
            }

            return builder;
        }
    }
}
