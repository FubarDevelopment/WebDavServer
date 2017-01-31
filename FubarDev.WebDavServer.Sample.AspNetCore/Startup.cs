using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.AspNetCore;
using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.DotNet;
using FubarDev.WebDavServer.Properties.Store;
using FubarDev.WebDavServer.Properties.Store.TextFile;
using FubarDev.WebDavServer.Sample.AspNetCore.BasicAuth;
using FubarDev.WebDavServer.Sample.AspNetCore.Support;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Formatters;
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
                .AddSingleton<IFileSystemFactory, TestFileSystemFactory>()
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

            /*
            if (Program.IsKestrel)
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
            */

            app.UseMiddleware<RequestLogMiddleware>();

            app.UseIdentity();
            app.UseForwardedHeaders(new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseMvc();
        }

        private class RequestLogMiddleware
        {
            private readonly IEnumerable<MediaType> _supportedMediaTypes = new[] { "text/xml", "application/xml" }.Select(x => new MediaType(x)).ToList();
            private readonly RequestDelegate _next;
            private readonly ILogger<RequestLogMiddleware> _logger;

            public RequestLogMiddleware(RequestDelegate next, ILogger<RequestLogMiddleware> logger)
            {
                _next = next;
                _logger = logger;
            }

            public async Task Invoke(HttpContext context)
            {
                using (_logger.BeginScope("RequestInfo"))
                {
                    var info = new List<string>()
                    {
                        $"{context.Request.Protocol} {context.Request.Method} {context.Request.GetDisplayUrl()}"
                    };

                    try
                    {
                        info.AddRange(context.Request.Headers.Select(x => $"{x.Key}: {x.Value}"));
                    }
                    catch
                    {
                        // Ignore all exceptions
                    }

                    if (context.Request.Body != null && !string.IsNullOrEmpty(context.Request.ContentType))
                    {
                        var contentType = new MediaType(context.Request.ContentType);
                        var isXml = _supportedMediaTypes.Any(x => contentType.IsSubsetOf(x));
                        if (isXml)
                        {
                            var temp = new MemoryStream();
                            await context.Request.Body.CopyToAsync(temp, 65536).ConfigureAwait(false);

                            if (temp.Length != 0)
                            {
                                temp.Position = 0;
                                var doc = XDocument.Load(temp);
                                info.Add($"Body: {doc}");

                                if (!context.Request.Body.CanSeek)
                                {
                                    var oldStream = context.Request.Body;
                                    context.Request.Body = temp;
                                    oldStream.Dispose();
                                }

                                context.Request.Body.Position = 0;
                            }
                        }
                    }

                    _logger.LogInformation(string.Join("\r\n", info));
                }

                await _next(context).ConfigureAwait(false);
            }
        }
    }
}
