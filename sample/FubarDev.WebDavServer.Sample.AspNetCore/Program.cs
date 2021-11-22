using System;
using System.Reflection;
using System.Runtime.InteropServices;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Mono.DllMap;

using Serilog;
using Serilog.Events;

namespace FubarDev.WebDavServer.Sample.AspNetCore
{
    public class Program
    {
        public static bool DisableLocking { get; private set; }

        public static bool IsKestrel { get; private set; }

        public static bool DisableBasicAuth { get; private set; }

        public static bool IsWindows => Environment.OSVersion.Platform == PlatformID.Win32NT;
        
        private static readonly DllMapResolver _dllMapResolver = new DllMapResolver();

        public static void Main(string[] args)
        {
            NativeLibrary.SetDllImportResolver(typeof(Npam.NpamSession).Assembly, Resolver);
            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("FubarDev.WebDavServer.BufferPools", LogEventLevel.Verbose)
#else
                .MinimumLevel.Warning()
#endif
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting web host");
                BuildWebHost(args).Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IntPtr Resolver(
            string libraryname,
            Assembly assembly,
            DllImportSearchPath? searchpath)
        {
            var resolvedLibraryName = _dllMapResolver.MapLibraryName<Program>(libraryname);
            return NativeLibrary.Load(resolvedLibraryName, assembly, searchpath);
        }

        private static IWebHost BuildWebHost(string[] args)
        {
            var tempHost = BuildWebHost(args, whb => whb);
            var config = tempHost.Services.GetRequiredService<IConfiguration>();

            DisableBasicAuth = config.GetValue<bool>("disable-basic-auth");
            DisableLocking = config.GetValue<bool>("disable-locking");

            return BuildWebHost(
                args,
                builder => ConfigureHosting(builder, config));
        }

        private static IWebHost BuildWebHost(string[] args, Func<IWebHostBuilder, IWebHostBuilder> customConfig) =>
            customConfig(WebHost.CreateDefaultBuilder(args))
                .UseStartup<Startup>()
                .UseSerilog()
                .Build();

        private static IWebHostBuilder ConfigureHosting(IWebHostBuilder builder, IConfiguration configuration)
        {
            var forceKestrelUse = configuration.GetValue<bool>("use-kestrel");
            if (!forceKestrelUse && IsWindows)
            {
                builder = builder
                    .UseIIS();
                /*
                .UseHttpSys(
                    opt =>
                    {
                        opt.Authentication.Schemes = AuthenticationSchemes.NTLM;
                        opt.Authentication.AllowAnonymous = true;
                        opt.MaxRequestBodySize = null;
                    }); */
            }
            else
            {
                builder = builder
                    .UseKestrel(opt => opt.Limits.MaxRequestBodySize = null);
                IsKestrel = true;
            }

            return builder;
        }
    }
}
