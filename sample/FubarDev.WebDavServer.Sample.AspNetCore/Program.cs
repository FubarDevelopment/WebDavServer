using System;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Sample.AspNetCore
{
    public class Program
    {
        public static bool IsKestrel { get; private set; }

        public static bool IsWindows => Environment.OSVersion.Platform == PlatformID.Win32NT;

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((ctxt, b) => { b.AddExceptionDemystifyer(); })
                .UseStartup<Startup>();

            var environmentName = builder.GetSetting("ENVIRONMENT");
            var config = Configure(new ConfigurationBuilder(), environmentName, args).Build();

            builder.UseConfiguration(config);

            var forceKestrelUse = config.GetValue<bool>("use-kestrel");
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

        private static IConfigurationBuilder Configure(
            IConfigurationBuilder config,
            string environmentName,
            string[] args)
        {
            config.AddJsonFile("appsettings.json", true, true);

            config.AddJsonFile(
                string.Format("appsettings.{0}.json", environmentName),
                true,
                true);

            if (environmentName == EnvironmentName.Development)
            {
                var assembly = typeof(Program).Assembly;
                config.AddUserSecrets(assembly, true);
            }

            config.AddEnvironmentVariables();
            if (args != null)
                config.AddCommandLine(args);

            return config;
        }
    }
}
