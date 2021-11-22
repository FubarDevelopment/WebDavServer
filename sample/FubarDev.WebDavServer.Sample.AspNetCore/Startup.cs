using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using FubarDev.WebDavServer.AspNetCore;
using FubarDev.WebDavServer.AspNetCore.Logging;
using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.DotNet;
using FubarDev.WebDavServer.FileSystem.InMemory;
using FubarDev.WebDavServer.FileSystem.SQLite;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Locking.InMemory;
using FubarDev.WebDavServer.Locking.SQLite;
using FubarDev.WebDavServer.Props.Store;
using FubarDev.WebDavServer.Props.Store.InMemory;
using FubarDev.WebDavServer.Props.Store.SQLite;
using FubarDev.WebDavServer.Props.Store.TextFile;
using FubarDev.WebDavServer.Sample.AspNetCore.Middlewares;

using idunno.Authentication.Basic;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            InMemory,
        }

        private enum PropertyStoreType
        {
            TextFile,
            SQLite,
            InMemory,
        }

        private enum LockManagerType
        {
            InMemory,
            SQLite,
        }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(
                    opt =>
                    {
                        if (!Program.IsKestrel || !Program.DisableBasicAuth)
                        {
                            opt.DefaultScheme = BasicAuthenticationDefaults.AuthenticationScheme;
                        }
                        else
                        {
                            opt.DefaultScheme = "Anonymous";
                        }

                        opt.AddScheme<Authentication.AnonymousAuthHandler>("Anonymous", null);
                    })
                .AddBasic(
                    opt =>
                    {
                        opt.Events = new BasicAuthenticationEvents()
                        {
                            OnValidateCredentials = ValidateCredentialsAsync,
                        };

                        opt.AllowInsecureProtocol = true;
                    });

            services.Configure<WebDavHostOptions>(cfg => Configuration.Bind("Host", cfg));

            services
                .AddMvcCore()
                .AddAuthorization()
                .AddWebDav(Program.DisableLocking);

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
                            opt => opt.RootPath = Path.Combine(Path.GetTempPath(), "webdav"))
                        .AddSingleton<IFileSystemFactory, SQLiteFileSystemFactory>();
                    break;
                case FileSystemType.InMemory:
                    services
                        .AddSingleton<IFileSystemFactory, InMemoryFileSystemFactory>();
                    break;
                default:
                    throw new NotSupportedException();
            }

            switch (serverConfig.PropertyStore)
            {
                case PropertyStoreType.TextFile:
                    services
                        .AddSingleton<IPropertyStoreFactory, TextFilePropertyStoreFactory>();
                    break;
                case PropertyStoreType.SQLite:
                    services
                        .AddSingleton<IPropertyStoreFactory, SQLitePropertyStoreFactory>();
                    services.Configure<SQLitePropertyStoreOptions>(opt => opt.EstimatedCost = 0);
                    break;
                case PropertyStoreType.InMemory:
                    services
                        .AddSingleton<IPropertyStoreFactory, InMemoryPropertyStoreFactory>();
                    break;
                default:
                    throw new NotSupportedException();
            }

            if (!Program.DisableLocking)
            {
                switch (serverConfig.LockManager)
                {
                    case LockManagerType.InMemory:
                        services
                            .AddSingleton<ILockManager, InMemoryLockManager>();
                        break;
                    case LockManagerType.SQLite:
                        services
                            .AddSingleton<ILockManager, SQLiteLockManager>()
                            .Configure<SQLiteLockManagerOptions>(
                                cfg => cfg.DatabaseFileName = Path.Combine(Path.GetTempPath(), "webdav", "locks.db"));
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseMiddleware<RequestLogMiddleware>();

            app.UseRouting();

            if (!Program.IsKestrel || !Program.DisableBasicAuth)
            {
                app.UseAuthentication();
            }

            if (!Program.IsKestrel)
            {
                app.UseMiddleware<ImpersonationMiddleware>();
            }

            app.UseAuthorization();

            app.UseEndpoints(
                routes => { routes.MapControllers(); });
        }

        private Task ValidateCredentialsAsync(ValidateCredentialsContext context)
        {
            if (Program.IsWindows)
            {
                return ValidateWindowsTestCredentialsAsync(context);
            }

            return ValidateLinuxTestCredentialsAsync(context);
        }

        private Task ValidateLinuxTestCredentialsAsync(ValidateCredentialsContext context)
        {
            if (!Npam.NpamUser.Authenticate("passwd", context.Username, context.Password))
                return HandleFailedAuthenticationAsync(context);

            var groups = Npam.NpamUser.GetGroups(context.Username).ToList();
            var accountInfo = Npam.NpamUser.GetAccountInfo(context.Username);

            var ticket = CreateAuthenticationTicket(accountInfo, groups);
            context.Principal = ticket.Principal;
            context.Properties = ticket.Properties;
            context.Success();

            return Task.CompletedTask;
        }

        private Task ValidateWindowsTestCredentialsAsync(ValidateCredentialsContext context)
        {
            var credentials = new List<AccountInfo>()
            {
                new AccountInfo() { Username = "tester", Password = "noGh2eefabohgohc", HomeDir = "c:\\temp\\tester" },
            }.ToDictionary(x => x.Username, StringComparer.OrdinalIgnoreCase);

            if (!credentials.TryGetValue(context.Username, out var accountInfo))
                return HandleFailedAuthenticationAsync(context);

            if (accountInfo.Password != context.Password)
            {
                context.Fail("Invalid password");
                return Task.CompletedTask;
            }

            var groups = Enumerable.Empty<Group>();

            var ticket = CreateAuthenticationTicket(accountInfo, groups);
            context.Principal = ticket.Principal;
            context.Properties = ticket.Properties;
            context.Success();

            return Task.CompletedTask;
        }

        private static Task HandleFailedAuthenticationAsync(ValidateCredentialsContext context, bool? allowAnonymousAccess = null, string authenticationScheme = "Basic")
        {
            if (context.Username != "anonymous")
                return Task.CompletedTask;

            var hostOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<WebDavHostOptions>>();
            var allowAnonAccess = allowAnonymousAccess ?? hostOptions.Value.AllowAnonymousAccess;
            if (!allowAnonAccess)
                return Task.CompletedTask;

            var groups = Enumerable.Empty<Group>();
            var accountInfo = new AccountInfo()
            {
                Username = context.Username,
                HomeDir = hostOptions.Value.AnonymousHomePath,
            };

            var ticket = CreateAuthenticationTicket(accountInfo, groups, "anonymous", authenticationScheme);
            context.Principal = ticket.Principal;
            context.Properties = ticket.Properties;
            context.Success();

            return Task.CompletedTask;
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

        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
        private class ServerConfiguration
        {
            public FileSystemType FileSystem { get; set; } = FileSystemType.DotNet;

            public PropertyStoreType PropertyStore { get; set; } = PropertyStoreType.TextFile;

            public LockManagerType LockManager { get; set; } = LockManagerType.InMemory;
        }
    }
}
