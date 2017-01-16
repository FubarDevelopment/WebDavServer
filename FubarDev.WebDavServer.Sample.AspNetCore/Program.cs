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

        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("hosting.json", true)
                .AddCommandLine(args)
                .Build();

            var forceKestrelUse = config["use-kestrel"] != null;

            IWebHost host;
            if (!forceKestrelUse && IsWindows())
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
                IsKestrel = true;
                host = new WebHostBuilder()
                    .UseKestrel()
                    .UseConfiguration(config)
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .Build();
            }

            host.Run();
        }

        public static bool IsWindows()
        {
            string windir = Environment.GetEnvironmentVariable("windir");
            return !string.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir);
        }
    }
}
