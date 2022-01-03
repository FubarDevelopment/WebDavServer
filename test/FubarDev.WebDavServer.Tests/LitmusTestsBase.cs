// <copyright file="LitmusTestsBase.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;

using DecaTec.WebDav;
using DecaTec.WebDav.Headers;
using DecaTec.WebDav.Tools;
using DecaTec.WebDav.WebDavArtifacts;

namespace FubarDev.WebDavServer.Tests
{
    public abstract class LitmusTestsBase : ServerTestsBase
    {
        private static readonly WebDavTimeoutHeaderValue _defaultTimeout =
            WebDavTimeoutHeaderValue.CreateWebDavTimeout(TimeSpan.FromHours(1));

        private static readonly LockScope _exclusive = LockScope.CreateExclusiveLockScope();

        private static readonly LockType _write = LockType.CreateWriteLockType();

        protected LitmusTestsBase()
        {
        }

        protected LitmusTestsBase(RecursiveProcessingMode processingMode)
            : base(processingMode)
        {
        }

        protected async Task InitAsync(string region, int index, string name, params string[] rootPathParts)
        {
            // Initialize client with default values
            Client.DefaultRequestHeaders.Add("X-Litmus", $"{region}: {index} ({name})");
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

        protected async Task<WebDavResponseMessage> LockAsync(string path, WebDavDepthHeaderValue depth)
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

        protected async Task<WebDavResponseMessage> UnlockAsync(string path, LockToken lockToken)
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

        protected WebDavClient CreateClone(string litmusHeaderName = "X-Litmus")
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
