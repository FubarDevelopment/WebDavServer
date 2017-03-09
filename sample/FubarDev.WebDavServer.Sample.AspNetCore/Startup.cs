using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using FubarDev.WebDavServer.AspNetCore;
using FubarDev.WebDavServer.AspNetCore.Logging;
using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.DotNet;
using FubarDev.WebDavServer.FileSystem.SQLite;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Locking.InMemory;
using FubarDev.WebDavServer.Locking.SQLite;
using FubarDev.WebDavServer.Props.Store;
using FubarDev.WebDavServer.Props.Store.SQLite;
using FubarDev.WebDavServer.Props.Store.TextFile;

using idunno.Authentication;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npam.Interop;

namespace FubarDev.WebDavServer.Sample.AspNetCore
{
    public class Startup
    {
        private enum FileSystemType
        {
            DotNet,
            SQLite,
        }

        private enum PropertyStoreType
        {
            TextFile,
            SQLite,
        }

        private enum LockManagerType
        {
            InMemory,
            SQLite,
        }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddCommandLine(Environment.GetCommandLineArgs().Skip(1).ToArray())
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
                .AddMvcCore()
                .AddAuthorization()
                .AddWebDav();

            var serverConfig = new ServerConfiguration();
            var serverConfigSection = Configuration.GetSection("Server");
            serverConfigSection?.Bind(serverConfig);

            switch (serverConfig.FileSystem)
            {
                case FileSystemType.DotNet:
                    services
                        .Configure<DotNetFileSystemOptions>(
                            opt =>
                            {
                                opt.RootPath = Path.Combine(Path.GetTempPath(), "webdav");
                                opt.AnonymousUserName = "anonymous";
                            })
                        .AddSingleton<IFileSystemFactory, DotNetFileSystemFactory>();
                    break;
                case FileSystemType.SQLite:
                    services
                        .Configure<SQLiteFileSystemOptions>(
                            opt =>
                            {
                                opt.RootPath = Path.Combine(Path.GetTempPath(), "webdav");
                            })
                        .AddSingleton<IFileSystemFactory, SQLiteFileSystemFactory>();
                    break;
                default:
                    throw new NotSupportedException();
            }

            switch (serverConfig.PropertyStore)
            {
                case PropertyStoreType.TextFile:
                    services
                        .AddTransient<IPropertyStoreFactory, TextFilePropertyStoreFactory>();
                    break;
                case PropertyStoreType.SQLite:
                    services
                        .AddTransient<IPropertyStoreFactory, SQLitePropertyStoreFactory>();
                    break;
                default:
                    throw new NotSupportedException();
            }

            switch (serverConfig.LockManager)
            {
                case LockManagerType.InMemory:
                    services
                        .AddTransient<ILockManager, InMemoryLockManager>();
                    break;
                case LockManagerType.SQLite:
                    services
                        .AddTransient<ILockManager, SQLiteLockManager>()
                        .Configure<SQLiteLockManagerOptions>(
                            cfg =>
                            {
                                cfg.DatabaseFileName = Path.Combine(Path.GetTempPath(), "webdav", "locks.db");
                            });
                    break;
                default:
                    throw new NotSupportedException();
            }
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

            if (Program.IsKestrel)
            {
                if (Program.DisableBasicAuth)
                {
                    app.UseMiddleware<AnonymousAuthenticationMiddleware>();
                }
                else
                {
                    app.UseBasicAuthentication(
                        new BasicAuthenticationOptions()
                        {
                            Events = new BasicAuthenticationEvents()
                            {
                                OnValidateCredentials = ValidateCredentialsAsync,
                            }
                        });
                }
            }

            app.UseMiddleware<RequestLogMiddleware>();

            app.UseForwardedHeaders(new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseMvc();
        }

        private Task ValidateCredentialsAsync(ValidateCredentialsContext context)
        {
            if (Program.IsWindows)
                return ValidateWindowsTestCredentialsAsync(context);

            return ValidateLinuxTestCredentialsAsync(context);
        }

        private Task ValidateLinuxTestCredentialsAsync(ValidateCredentialsContext context)
        {
            if (!Npam.NpamUser.Authenticate("passwd", context.Username, context.Password))
                return HandleFailedAuthenticationAsync(context);

            var groups = Npam.NpamUser.GetGroups(context.Username).ToList();
            var accountInfo = Npam.NpamUser.GetAccountInfo(context.Username);

            context.Ticket = CreateAuthenticationTicket(accountInfo, groups);
            context.HandleResponse();

            return Task.FromResult(0);
        }

        private Task ValidateWindowsTestCredentialsAsync(ValidateCredentialsContext context)
        {
            var credentials = new List<AccountInfo>()
            {
                new AccountInfo() { Username = "tester", Password = "noGh2eefabohgohc", HomeDir = "c:\\temp\\tester" },
            }.ToDictionary(x => x.Username, StringComparer.OrdinalIgnoreCase);

            AccountInfo accountInfo;
            if (!credentials.TryGetValue(context.Username, out accountInfo))
                return HandleFailedAuthenticationAsync(context);

            if (accountInfo.Password != context.Password)
            {
                context.HandleResponse();
                return Task.FromResult(0);
            }

            var groups = Enumerable.Empty<Group>();

            context.Ticket = CreateAuthenticationTicket(accountInfo, groups);
            context.HandleResponse();

            return Task.FromResult(0);
        }

        private static Task HandleFailedAuthenticationAsync(ValidateCredentialsContext context, bool? allowAnonymousAccess = null, string authenticationScheme = "Basic")
        {
            if (context.Username != "anonymous")
                return Task.FromResult(0);

            var hostOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<WebDavHostOptions>>();
            var allowAnonAccess = allowAnonymousAccess ?? hostOptions.Value.AllowAnonymousAccess;
            if (!allowAnonAccess)
                return Task.FromResult(0);

            var groups = Enumerable.Empty<Group>();
            var accountInfo = new AccountInfo()
            {
                Username = context.Username,
                HomeDir = hostOptions.Value.AnonymousHomePath,
            };

            context.Ticket = CreateAuthenticationTicket(accountInfo, groups, "anonymous", authenticationScheme);
            context.HandleResponse();

            return Task.FromResult(0);
        }

        private static AuthenticationTicket CreateAuthenticationTicket(AccountInfo accountInfo, IEnumerable<Group> groups, string authenticationType = "passwd", string authenticationScheme = "Basic")
        {
            var claims = new List<Claim>();
            if (!string.IsNullOrEmpty(accountInfo.HomeDir))
                claims.Add(new Claim(Utils.SystemInfo.UserHomePathClaim, accountInfo.HomeDir));
            claims.Add(new Claim(ClaimsIdentity.DefaultNameClaimType, accountInfo.Username));
            claims.AddRange(groups.Select(x => new Claim(ClaimsIdentity.DefaultRoleClaimType, x.GroupName)));

            var identity = new ClaimsIdentity(claims, authenticationType);
            var principal = new ClaimsPrincipal(identity);
            return new AuthenticationTicket(principal, new AuthenticationProperties(), authenticationScheme);
        }

        private class AnonymousAuthenticationOptions : AuthenticationOptions
        {
            public AnonymousAuthenticationOptions()
            {
                AutomaticChallenge = true;
                AutomaticAuthenticate = true;
                AuthenticationScheme = "Anonymous";
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class AnonymousAuthenticationMiddleware : AuthenticationMiddleware<AnonymousAuthenticationOptions>
        {
            public AnonymousAuthenticationMiddleware(RequestDelegate next, IOptions<AnonymousAuthenticationOptions> options, ILoggerFactory loggerFactory, UrlEncoder encoder) : base(next, options, loggerFactory, encoder)
            {
            }

            protected override AuthenticationHandler<AnonymousAuthenticationOptions> CreateHandler()
            {
                return new Handler(Options);
            }

            private class Handler : AuthenticationHandler<AnonymousAuthenticationOptions>
            {
                private readonly AnonymousAuthenticationOptions _options;

                public Handler(AnonymousAuthenticationOptions options)
                {
                    _options = options ?? new AnonymousAuthenticationOptions();
                }

                protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
                {
                    var context = new ValidateCredentialsContext(Context, new BasicAuthenticationOptions())
                    {
                        Username = "anonymous",
                    };

                    await HandleFailedAuthenticationAsync(context, true, _options.AuthenticationScheme).ConfigureAwait(false);
                    Debug.Assert(context.Ticket != null);

                    var result = AuthenticateResult.Success(context.Ticket);
                    return result;
                }
            }
        }

        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
        private class ServerConfiguration
        {
            public FileSystemType FileSystem { get; set; } = FileSystemType.DotNet;

            public PropertyStoreType PropertyStore { get; set; } = PropertyStoreType.TextFile;

            public LockManagerType LockManager { get; set; } = LockManagerType.InMemory;
        }
    }
}
