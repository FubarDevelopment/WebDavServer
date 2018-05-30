using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using FluentMigrator.Runner;

using FubarDev.WebDavServer.AspNetCore;
using FubarDev.WebDavServer.AspNetCore.Logging;
using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.DotNet;
using FubarDev.WebDavServer.FileSystem.SQLite;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Locking.InMemory;
using FubarDev.WebDavServer.Locking.SQLite;
using FubarDev.WebDavServer.NHibernate.FileSystem;
using FubarDev.WebDavServer.NHibernate.Locking;
using FubarDev.WebDavServer.NHibernate.Models;
using FubarDev.WebDavServer.NHibernate.Props.Store;
using FubarDev.WebDavServer.Props.Store;
using FubarDev.WebDavServer.Props.Store.SQLite;
using FubarDev.WebDavServer.Props.Store.TextFile;
using FubarDev.WebDavServer.Sample.AspNetCore.Middlewares;

using idunno.Authentication;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NHibernate;

using Npam.Interop;

namespace FubarDev.WebDavServer.Sample.AspNetCore
{
    public class Startup
    {
        private const string NHibernateConfigurationErrorMessage = "Either all or none of the modules (file system, lock manager, property store) must be configured to use NHibernate.";
        private enum FileSystemType
        {
            Default,
            DotNet,
            SQLite,
            NHibernate,
        }

        private enum PropertyStoreType
        {
            Default,
            TextFile,
            SQLite,
            NHibernate,
        }

        private enum LockManagerType
        {
            Default,
            InMemory,
            SQLite,
            NHibernate,
        }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(
                    opt =>
                    {
                        if (!Program.IsKestrel || !Program.DisableBasicAuth)
                        {
                            opt.DefaultScheme = "Basic";
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
                        opt.Events.OnValidateCredentials = ValidateCredentialsAsync;
                        opt.AllowInsecureProtocol = true;
                    });

            services.Configure<WebDavHostOptions>(cfg => Configuration.Bind("Host", cfg));

            services
                .AddMvcCore()
                .AddAuthorization()
                .AddWebDav();

            var serverConfig = new ServerConfiguration();
            var serverConfigSection = Configuration.GetSection("Server");
            serverConfigSection?.Bind(serverConfig);

            var isNHibernateSelected =
                serverConfig.FileSystem == FileSystemType.NHibernate
                || serverConfig.LockManager == LockManagerType.NHibernate
                || serverConfig.PropertyStore == PropertyStoreType.NHibernate;

            if (isNHibernateSelected)
            {
                if (serverConfig.FileSystem == FileSystemType.Default)
                {
                    serverConfig.FileSystem = FileSystemType.NHibernate;
                }
                else if (serverConfig.FileSystem != FileSystemType.NHibernate)
                {
                    throw new NotSupportedException(NHibernateConfigurationErrorMessage);
                }

                if (serverConfig.LockManager != LockManagerType.NHibernate &&
                    serverConfig.LockManager != LockManagerType.Default)
                {
                    throw new NotSupportedException(NHibernateConfigurationErrorMessage);
                }

                if (serverConfig.PropertyStore != PropertyStoreType.NHibernate &&
                    serverConfig.PropertyStore != PropertyStoreType.Default)
                {
                    throw new NotSupportedException(NHibernateConfigurationErrorMessage);
                }
            }

            if (isNHibernateSelected)
            {
                var sqliteDbFileName = Path.Combine(Path.GetTempPath(), "webdav.db");
                var csb = new SqliteConnectionStringBuilder()
                {
                    DataSource = sqliteDbFileName,
                };

                var connectionString = csb.ConnectionString;
                var cfg = new global::NHibernate.Cfg.Configuration();
                cfg.SetProperty(global::NHibernate.Cfg.Environment.Dialect, "NHibernate.Dialect.SQLiteDialect");
                cfg.SetProperty(global::NHibernate.Cfg.Environment.ConnectionString, connectionString);
                cfg.SetProperty(global::NHibernate.Cfg.Environment.ConnectionDriver, "FubarDev.WebDavServer.Sample.AspNetCore.NhSupport.MicrosoftDataSqliteDriver, FubarDev.WebDavServer.Sample.AspNetCore");
                cfg.AddAssembly(typeof(NHibernateFileSystem).Assembly);

                var sessionFactory = cfg.BuildSessionFactory();

                services
                    .AddSingleton(sessionFactory)
                    .AddScoped(sp => sp.GetRequiredService<ISessionFactory>().OpenSession())
                    .AddScoped(sp => sp.GetRequiredService<ISessionFactory>().OpenStatelessSession());

                services
                    .AddFluentMigratorCore()
                    .ConfigureRunner(
                        rb =>
                        {
                            rb.AddSQLite();
                            rb.WithMigrationsIn(typeof(NHibernateFileSystem).Assembly);
                            rb.WithGlobalConnectionString(connectionString);
                        });
            }

            switch (serverConfig.FileSystem)
            {
                case FileSystemType.Default:
                case FileSystemType.DotNet:
                    services
                        .Configure<DotNetFileSystemOptions>(
                            opt =>
                            {
                                opt.RootPath = Path.Combine(Path.GetTempPath(), "webdav");
                                opt.AnonymousUserName = "anonymous";
                            })
                        .AddScoped<IFileSystemFactory, DotNetFileSystemFactory>();
                    break;
                case FileSystemType.SQLite:
                    services
                        .Configure<SQLiteFileSystemOptions>(
                            opt =>
                            {
                                opt.RootPath = Path.Combine(Path.GetTempPath(), "webdav");
                            })
                        .AddScoped<IFileSystemFactory, SQLiteFileSystemFactory>();
                    break;
                case FileSystemType.NHibernate:
                    services
                        .AddScoped<IFileSystemFactory, NHibernateFileSystemFactory>();
                    break;

                default:
                    throw new NotSupportedException();
            }

            switch (serverConfig.PropertyStore)
            {
                case PropertyStoreType.Default:
                    switch (serverConfig.FileSystem)
                    {
                        case FileSystemType.SQLite:
                            services
                                .AddScoped<IPropertyStoreFactory, SQLitePropertyStoreFactory>();
                            break;

                        case FileSystemType.NHibernate:
                            services
                                .AddScoped<IPropertyStoreFactory, NHibernatePropertyStoreFactory>();
                            break;

                        default:
                            services
                                .AddScoped<IPropertyStoreFactory, TextFilePropertyStoreFactory>();
                            break;
                    }
                    break;

                case PropertyStoreType.TextFile:
                    services
                        .AddScoped<IPropertyStoreFactory, TextFilePropertyStoreFactory>();
                    break;
                case PropertyStoreType.SQLite:
                    services
                        .AddScoped<IPropertyStoreFactory, SQLitePropertyStoreFactory>();
                    break;
                case PropertyStoreType.NHibernate:
                    services
                        .AddScoped<IPropertyStoreFactory, NHibernatePropertyStoreFactory>();
                    break;
                default:
                    throw new NotSupportedException();
            }

            switch (serverConfig.LockManager)
            {
                case LockManagerType.Default:
                    switch (serverConfig.FileSystem)
                    {
                        case FileSystemType.SQLite:
                            services
                                .AddSingleton<ILockManager, SQLiteLockManager>()
                                .Configure<SQLiteLockManagerOptions>(
                                    cfg =>
                                    {
                                        cfg.DatabaseFileName = Path.Combine(Path.GetTempPath(), "webdav", "locks.db");
                                    });
                            break;

                        case FileSystemType.NHibernate:
                            services
                                .AddSingleton<ILockManager, NHibernateLockManager>();
                            break;

                        default:
                            services
                                .AddSingleton<ILockManager, InMemoryLockManager>();
                            break;
                    }

                    break;

                case LockManagerType.InMemory:
                    services
                        .AddSingleton<ILockManager, InMemoryLockManager>();
                    break;
                case LockManagerType.SQLite:
                    services
                        .AddSingleton<ILockManager, SQLiteLockManager>()
                        .Configure<SQLiteLockManagerOptions>(
                            cfg =>
                            {
                                cfg.DatabaseFileName = Path.Combine(Path.GetTempPath(), "webdav", "locks.db");
                            });
                    break;
                case LockManagerType.NHibernate:
                    services
                        .AddSingleton<ILockManager, NHibernateLockManager>();
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var runner = scope.ServiceProvider.GetService<IMigrationRunner>();
                if (runner != null)
                {
                    runner.MigrateUp();

                    // Seeding
                    var session = scope.ServiceProvider.GetRequiredService<IStatelessSession>();
                    var rootFileEntry = session.Get<FileEntry>(Guid.Empty);
                    if (rootFileEntry == null)
                    {
                        var now = DateTime.UtcNow;
                        rootFileEntry = new FileEntry()
                        {
                            Name = string.Empty,
                            InvariantName = string.Empty,
                            IsCollection = true,
                            LastWriteTimeUtc = now,
                            CreationTimeUtc = now,
                            Properties = new Dictionary<string, PropertyEntry>()
                        };

                        session.Insert(rootFileEntry);
                    }
                }
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseExceptionDemystifier();
            }

            if (!Program.IsKestrel || !Program.DisableBasicAuth)
            {
                app.UseAuthentication();
            }

            app.UseMiddleware<RequestLogMiddleware>();

            if (!Program.IsKestrel)
            {
                app.UseMiddleware<ImpersonationMiddleware>();
            }

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
            public FileSystemType FileSystem { get; set; } = FileSystemType.Default;

            public PropertyStoreType PropertyStore { get; set; } = PropertyStoreType.Default;

            public LockManagerType LockManager { get; set; } = LockManagerType.Default;
        }
    }
}
