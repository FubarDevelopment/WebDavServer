using System;
using System.Reflection;
using System.Runtime.InteropServices;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using Mono.DllMap;

using Serilog;
using Serilog.Events;

namespace FubarDev.WebDavServer.Sample.AspNetCore
{
    public class Program
    {
        public static bool IsWindows => Environment.OSVersion.Platform == PlatformID.Win32NT;

        private static readonly DllMapResolver _dllMapResolver = new();

        public static void Main(string[] args)
        {
            NativeLibrary.SetDllImportResolver(typeof(Npam.NpamSession).Assembly, Resolver);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
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

        private static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                .UseSerilog(
                    (context, services, configuration) =>
                    {
                        configuration
                            .ReadFrom.Configuration(context.Configuration)
                            .ReadFrom.Services(services)
                            .Enrich.WithClientIp()
                            .Enrich.WithClientAgent()
                            .Enrich.FromLogContext()
                            .WriteTo.Console();
                    })
                .ConfigureWebHostDefaults(
                    builder =>
                    {
                        builder
                            .UseStartup<Startup>();
                    });
    }
}
