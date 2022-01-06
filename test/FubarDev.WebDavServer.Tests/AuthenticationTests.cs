// <copyright file="AuthenticationTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using DecaTec.WebDav;
using DecaTec.WebDav.Headers;
using DecaTec.WebDav.Tools;
using DecaTec.WebDav.WebDavArtifacts;

using FubarDev.WebDavServer.Tests.Support.Controllers;

using idunno.Authentication.Basic;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace FubarDev.WebDavServer.Tests
{
    public class AuthenticationTests : ServerTestsBase
    {
        protected override IEnumerable<Type> ControllerTypes { get; } = new[]
        {
            typeof(SimpleAuthWebDavController),
        };

        [Fact]
        public async Task CheckFailedAuthAsync()
        {
            (await Client.PropFindAsync(string.Empty))
                .EnsureStatusCode(StatusCode.Unauthorized);
        }

        [Fact]
        public async Task CheckSuccessAuthWithUserAAsync()
        {
            Client.UseAuthentication("user-a", "password-a");
            (await Client.PropFindAsync(string.Empty))
                .EnsureStatusCode(StatusCode.MultiStatus);
        }

        [Fact]
        public async Task DeleteLockedWithDifferentUserWithoutTokenShouldFail()
        {
            var ct = CancellationToken.None;
            var root = await GetFileSystem().Root;
            var coll = await root.CreateCollectionAsync("coll", ct);
            await coll.CreateDocumentAsync("test.txt", ct);

            // Create a lock
            var client1 = Client.UseAuthentication("user-a", "password-a");
            var lockResponse = (await client1.LockAsync(
                    "coll",
                    WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout(),
                    WebDavDepthHeaderValue.Infinity,
                    new LockInfo()
                    {
                        LockScope = LockScope.CreateExclusiveLockScope(),
                        LockType = LockType.CreateWriteLockType(),
                    },
                    ct))
                .EnsureSuccess();
            var lockToken = await WebDavHelper.GetLockTokenFromWebDavResponseMessage(lockResponse);
            Assert.NotNull(lockToken);

            using var client2 = new WebDavClient(Server.CreateHandler())
            {
                BaseAddress = Server.BaseAddress,
            };

            client2.UseAuthentication("user-b", "password-b");
            (await client2.DeleteAsync("coll/test.txt", ct))
                .EnsureStatusCode(StatusCode.Locked);
        }

        [Fact]
        public async Task DeleteLockedWithDifferentUserWithTokenShouldFail()
        {
            var ct = CancellationToken.None;
            var root = await GetFileSystem().Root;
            var coll = await root.CreateCollectionAsync("coll", ct);
            await coll.CreateDocumentAsync("test.txt", ct);

            // Create a lock
            var client1 = Client.UseAuthentication("user-a", "password-a");
            var lockResponse = (await client1.LockAsync(
                    "coll",
                    WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout(),
                    WebDavDepthHeaderValue.Infinity,
                    new LockInfo()
                    {
                        LockScope = LockScope.CreateExclusiveLockScope(),
                        LockType = LockType.CreateWriteLockType(),
                    },
                    ct))
                .EnsureSuccess();
            var lockToken = await WebDavHelper.GetLockTokenFromWebDavResponseMessage(lockResponse);
            Assert.NotNull(lockToken);

            using var client2 = new WebDavClient(Server.CreateHandler())
            {
                BaseAddress = Server.BaseAddress,
            };

            client2.UseAuthentication("user-b", "password-b");
            (await client2.DeleteAsync("coll/test.txt", lockToken, ct))
                .EnsureStatusCode(StatusCode.Locked);
        }

        /// <inheritdoc />
        protected override void ConfigureServices(IServiceCollection services)
        {
            services
                .AddAuthentication(
                    opt => { opt.DefaultScheme = BasicAuthenticationDefaults.AuthenticationScheme; })
                .AddBasic(
                    opt =>
                    {
                        opt.Events = new BasicAuthenticationEvents()
                        {
                            OnValidateCredentials = ValidateCredentialsAsync,
                        };

                        opt.AllowInsecureProtocol = true;
                    });
        }

        private Task ValidateCredentialsAsync(ValidateCredentialsContext context)
        {
            var credentials = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["user-a"] = "password-a",
                ["user-b"] = "password-b",
            };

            if (!credentials.TryGetValue(context.Username, out var password))
            {
                return Task.CompletedTask;
            }

            if (password != context.Password)
            {
                context.Fail("Invalid password");
                return Task.CompletedTask;
            }

            var claims = new List<Claim>
            {
                new(ClaimsIdentity.DefaultNameClaimType, context.Username),
            };

            var identity = new ClaimsIdentity(claims, "anonymous");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(
                principal,
                new AuthenticationProperties(),
                BasicAuthenticationDefaults.AuthenticationScheme);

            context.Principal = ticket.Principal;
            context.Properties = ticket.Properties;
            context.Success();

            return Task.CompletedTask;
        }
    }
}
