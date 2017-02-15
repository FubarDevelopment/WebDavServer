using System.IO;

using FubarDev.WebDavServer.AspNetCore;
using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.DotNet;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Locking.InMemory;
using FubarDev.WebDavServer.Props.Store;
using FubarDev.WebDavServer.Props.Store.TextFile;
using FubarDev.WebDavServer.Sample.AspNetCore.BasicAuth;
using FubarDev.WebDavServer.Sample.AspNetCore.Support;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Sample.AspNetCore
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddOptions()
                .AddAuthentication()
                .Configure<WebDavHostOptions>(
                    opt =>
                    {
                        var hostSection = Configuration.GetSection("Host");
                        hostSection?.Bind(opt);
                    })
                .Configure<DotNetFileSystemOptions>(
                    opt =>
                    {
                        opt.RootPath = Path.Combine(Path.GetTempPath(), "webdav");
                        opt.AnonymousUserName = "anonymous";
                        opt.AllowInfiniteDepth = true;
                    })
                .Configure<TextFilePropertyStoreOptions>(
                    opt =>
                    {
                        opt.StoreInTargetFileSystem = true;
                    })
                .AddMemoryCache()
                .AddTransient<IPropertyStoreFactory, TextFilePropertyStoreFactory>()
                .AddSingleton<IFileSystemFactory, DotNetFileSystemFactory>()
                .AddSingleton<ILockManager, InMemoryLockManager>()
                .AddTransient(sp =>
                {
                    var factory = sp.GetRequiredService<IFileSystemFactory>();
                    var context = sp.GetRequiredService<IHttpContextAccessor>();
                    return factory.CreateFileSystem(context.HttpContext.User.Identity);
                })
                .AddMvcCore()
                .AddAuthorization()
                .AddWebDav();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (Program.IsKestrel && !Program.IsWindows)
            {
                app.UseBasicAuthentication(
                    confg =>
                    {
                        confg.Credentials = new[]
                        {
                            new BasicCredential() {Username = "tester", Password = "noGh2eefabohgohc"}
                        };
                    });
            }

            app.UseMiddleware<RequestLogMiddleware>();

            app.UseForwardedHeaders(new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseMvc();
        }
    }
}
