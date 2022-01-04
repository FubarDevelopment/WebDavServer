// <copyright file="Locks.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using DecaTec.WebDav;
using DecaTec.WebDav.Headers;
using DecaTec.WebDav.Tools;
using DecaTec.WebDav.WebDavArtifacts;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.SQLite;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Locking.SQLite;
using FubarDev.WebDavServer.Props.Store;
using FubarDev.WebDavServer.Props.Store.SQLite;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace FubarDev.WebDavServer.Tests.LitmusTolsen
{
    public class Locks : LitmusTestsBase
    {
        private const string FooContent = @"This
is
a
test
file
called
foo

";

        private readonly string _dataPath = CreateDataPath();

        [Fact]
        public async Task UnmapLockRoot()
        {
            await InitAsync("locks", 66, "unmap_lockroot", "lockcoll");

            var secondClient = CreateClone("X-Litmus-Second");

            (await Client.MkcolAsync("collX")).EnsureSuccessStatusCode();
            (await Client.MkcolAsync("collY")).EnsureSuccessStatusCode();

            // A depth of 0 shouldn't avoid creating a document in a sub-collection
            {
                var response = (await LockAsync(string.Empty, WebDavDepthHeaderValue.Zero)).EnsureSuccess();
                var lockToken = await WebDavHelper.GetLockTokenFromWebDavResponseMessage(response);
                Assert.NotNull(lockToken);
                (await secondClient.PutAsync("collX/conflict.txt", new StringContent(FooContent)))
                    .EnsureStatusCode(WebDavStatusCode.Created);
                (await UnlockAsync(string.Empty, lockToken)).EnsureSuccess();
                (await Client.DeleteAsync("collX/conflict.txt")).EnsureSuccess();
            }

            // A infinite depth must cause a failure when creating a document in a sub-collection
            {
                var response = (await LockAsync(string.Empty, WebDavDepthHeaderValue.Infinity)).EnsureSuccess();
                var lockToken = await WebDavHelper.GetLockTokenFromWebDavResponseMessage(response);
                Assert.NotNull(lockToken);
                (await secondClient.PutAsync("collX/conflict.txt", new StringContent(FooContent)))
                    .EnsureStatusCode(WebDavStatusCode.Locked);
                (await Client.PutAsync("collX/conflict.txt", new StringContent(FooContent)))
                    .EnsureStatusCode(WebDavStatusCode.Created);
                (await UnlockAsync(string.Empty, lockToken)).EnsureSuccess();
                (await Client.DeleteAsync("collX/conflict.txt")).EnsureSuccess();
            }

            // Test moving a locked file
            {
                (await Client.PutAsync("collX/conflict.txt", new StringContent(FooContent)))
                    .EnsureStatusCode(WebDavStatusCode.Created);
                var response = (await LockAsync("collX/conflict.txt", WebDavDepthHeaderValue.Infinity)).EnsureSuccess();
                var lockToken = await WebDavHelper.GetLockTokenFromWebDavResponseMessage(response);
                Assert.NotNull(lockToken);

                // This should fail because the file is locked
                (await secondClient.MoveAsync(
                        new Uri("collY", UriKind.Relative),
                        new Uri(secondClient.BaseAddress!, "collX"),
                        true))
                    .EnsureStatusCode(WebDavStatusCode.Locked);

                // This should work
                (await Client.MoveAsync(
                        new Uri("collY", UriKind.Relative),
                        new Uri(Client.BaseAddress!, "collX"),
                        true))
                    .EnsureSuccess();

                // The file (and its lock) must be gone
                (await Client.PropFindAsync(
                        "collX/conflict.txt",
                        WebDavDepthHeaderValue.Zero,
                        new PropFind()
                        {
                            Item = new Prop()
                            {
                                LockDiscovery = new LockDiscovery(),
                            },
                        }))
                    .EnsureStatusCode(WebDavStatusCode.NotFound);
            }

            // Test if the lock was removed during the MOVE
            var lockManager = Server.Services.GetRequiredService<ILockManager>();
            var locks = (await lockManager.GetLocksAsync()).ToList();
            Assert.Empty(locks);

            (await Client.DeleteAsync("collX")).EnsureSuccess();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            try
            {
                Directory.Delete(_dataPath, true);
            }
            catch
            {
                // Ignore
            }
        }

        /// <inheritdoc />
        protected override void ConfigureServices(IServiceCollection services)
        {
            services
                .Configure<SQLiteFileSystemOptions>(opt => opt.RootPath = _dataPath)
                .Configure<SQLiteLockManagerOptions>(opt => opt.DatabaseFileName = Path.Combine(_dataPath, "locks.db"))
                .AddSingleton<IFileSystemFactory, SQLiteFileSystemFactory>()
                .AddSingleton<IPropertyStoreFactory, SQLitePropertyStoreFactory>()
                .AddSingleton<ILockManager, SQLiteLockManager>();
        }

        private static string CreateDataPath()
        {
            var tempRootPath = Path.Combine(
                Path.GetTempPath(),
                "webdavserver-sqlite-tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempRootPath);
            return tempRootPath;
        }
    }
}
