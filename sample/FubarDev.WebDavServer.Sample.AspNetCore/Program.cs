using System;
using System.IO;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Server;

namespace FubarDev.WebDavServer.Sample.AspNetCore
{
    public class Program
    {
        public static bool IsKestrel { get; private set; }

        public static bool DisableBasicAuth { get; private set; }

        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("hosting.json", true)
                .AddCommandLine(args)
                .Build();

            var forceKestrelUse = config.GetValue<bool>("use-kestrel");
            DisableBasicAuth = config.GetValue<bool>("disable-basic-auth");

            IWebHost host;
            if (!forceKestrelUse && IsWindows)
            {
                host = new WebHostBuilder()
                    .UseWebListener(opt =>
                    {
                        opt.ListenerSettings.Authentication.Schemes = AuthenticationSchemes.NTLM;
                        opt.ListenerSettings.Authentication.AllowAnonymous = true;
                    })
                    .UseConfiguration(config)
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseStartup<Startup>()
                    .Build();
            }
            else
            {
                var pfxFileName = config.GetValue<string>("use-pfx");
                IsKestrel = true;
                host = new WebHostBuilder()
                    .UseKestrel(
                        opt =>
                        {
                            if (!string.IsNullOrEmpty(pfxFileName))
                            {
                                opt.UseHttps(pfxFileName);
                            }
                        })
                    .UseConfiguration(config)
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .Build();
            }

            host.Run();
        }

        public static bool IsWindows
        {
            get
            {
                string windir = Environment.GetEnvironmentVariable("windir");
                return !string.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir);
            }
        }
    }
}
