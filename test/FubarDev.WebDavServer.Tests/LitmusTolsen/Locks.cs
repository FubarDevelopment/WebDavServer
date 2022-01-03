// <copyright file="Locks.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using DecaTec.WebDav;
using DecaTec.WebDav.Headers;
using DecaTec.WebDav.Tools;
using DecaTec.WebDav.WebDavArtifacts;

using FubarDev.WebDavServer.Locking;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace FubarDev.WebDavServer.Tests.LitmusTolsen
{
    public class Locks : ServerTestsBase
    {
        private const string FooContent = @"This
is
a
test
file
called
foo

";

        private static readonly WebDavTimeoutHeaderValue _defaultTimeout =
            WebDavTimeoutHeaderValue.CreateWebDavTimeout(TimeSpan.FromHours(1));

        private static readonly LockScope _exclusive = LockScope.CreateExclusiveLockScope();

        private static readonly LockType _write = LockType.CreateWriteLockType();

        [Fact]
        public async Task UnmapLockRoot()
        {
            await InitAsync(66, "unmap_lockroot", "lockcoll");

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
                        new Uri("collX", UriKind.Relative),
                        new Uri(secondClient.BaseAddress!, "collY"),
                        true))
                    .EnsureStatusCode(WebDavStatusCode.Locked);

                // This should work
                (await Client.MoveAsync(
                        new Uri("collX", UriKind.Relative),
                        new Uri(secondClient.BaseAddress!, "collY"),
                        true))
                    .EnsureSuccess();

                // The file must be gone
                (await Client.PropFindAsync("collX/conflict.txt", WebDavDepthHeaderValue.Zero))
                    .EnsureStatusCode(WebDavStatusCode.NotFound);
            }

            // Test if the lock was removed during the MOVE
            var lockManager = Server.Services.GetRequiredService<ILockManager>();
            var locks = (await lockManager.GetLocksAsync()).ToList();
            Assert.Empty(locks);
        }

        private async Task InitAsync(int index, string name, params string[] rootPathParts)
        {
            // Initialize client with default values
            Client.DefaultRequestHeaders.Add("X-Litmus", $"locks: {index} ({name})");
            (await Client.MkcolAsync(new Uri("litmus", UriKind.Relative))).EnsureSuccessStatusCode();

            // Create the root path
            var baseAddress = new Uri(Client.BaseAddress!, new Uri("litmus/", UriKind.Relative));
            foreach (var rootPathPart in rootPathParts)
            {
                var oldClient = Client;
                Client = CreateClone();
                Client.BaseAddress = baseAddress;
                oldClient.Dispose();

                (await Client.MkcolAsync(new Uri(rootPathPart, UriKind.Relative))).EnsureSuccessStatusCode();
                baseAddress = new Uri(baseAddress, rootPathPart + "/");
            }

            {
                var oldClient = Client;
                Client = CreateClone();
                Client.BaseAddress = baseAddress;
                oldClient.Dispose();
            }
        }

        private async Task<WebDavResponseMessage> LockAsync(string path, WebDavDepthHeaderValue depth)
        {
            var response = await Client.LockAsync(
                path,
                _defaultTimeout,
                depth,
                new LockInfo()
                {
                    LockScope = _exclusive,
                    LockType = _write,
                    OwnerHref = "litmus test suite",
                });

            // Add lock token as default header to the client.
            if (response.IsSuccessStatusCode)
            {
                var lockToken = await WebDavHelper.GetLockTokenFromWebDavResponseMessage(response);
                if (lockToken != null)
                {
                    Client.DefaultRequestHeaders.Add("If", lockToken.IfHeaderNoTagListFormat.ToString());
                }
            }

            return response;
        }

        private async Task<WebDavResponseMessage> UnlockAsync(string path, LockToken lockToken)
        {
            // Remove the lock token from the default header.
            var lockTokenHeader = lockToken.IfHeaderNoTagListFormat.ToString();
            var headers = Client.DefaultRequestHeaders.GetValues("If")
                .Where(x => x != lockTokenHeader)
                .ToList();
            Client.DefaultRequestHeaders.Remove("If");
            if (headers.Count != 0)
            {
                Client.DefaultRequestHeaders.Add("If", headers);
            }

            // Perform the unlock. The lock token is automatically added to the request header.
            var response = await Client.UnlockAsync(path, lockToken);

            // Add the lock token to the header in case of an error.
            if (!response.IsSuccessStatusCode)
            {
                Client.DefaultRequestHeaders.Add("If", lockTokenHeader);
            }

            return response;
        }

        private WebDavClient CreateClone(string litmusHeaderName = "X-Litmus")
        {
            var client = new WebDavClient(Server.CreateHandler())
            {
                BaseAddress = Client.BaseAddress,
            };

            if (Client.DefaultRequestHeaders.TryGetValues("X-Litmus", out var litmusHeader))
            {
                client.DefaultRequestHeaders.Add(litmusHeaderName, litmusHeader);
            }

            return client;
        }
    }
}
