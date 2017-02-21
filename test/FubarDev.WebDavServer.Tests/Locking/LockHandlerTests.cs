// <copyright file="LockHandlerTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using System.Xml.Linq;

using DecaTec.WebDav;
using DecaTec.WebDav.WebDavArtifacts;

using Xunit;

namespace FubarDev.WebDavServer.Tests.Locking
{
    public class LockHandlerTests : ServerTestsBase
    {
        [Fact]
        public async Task AddLockToRootRecursiveMinimumTest()
        {
            var response = await Client.LockAsync(
                "/",
                WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout(),
                WebDavDepthHeaderValue.Infinity,
                new LockInfo()
                {
                    LockScope = LockScope.CreateExclusiveLockScope(),
                    LockType = LockType.CreateWriteLockType(),
                }).ConfigureAwait(false);
            var prop = await response.EnsureSuccessStatusCode()
                .Content.ParsePropResponseContentAsync().ConfigureAwait(false);
            Assert.NotNull(prop.LockDiscovery);
            Assert.Collection(
                prop.LockDiscovery.ActiveLock,
                activeLock =>
                {
                    Assert.Equal("/", activeLock.LockRoot.Href);
                    Assert.Equal(WebDavDepthHeaderValue.Infinity.ToString(), activeLock.Depth, StringComparer.OrdinalIgnoreCase);
                    Assert.IsType<Exclusive>(activeLock.LockScope.Item);
                    Assert.Null(activeLock.Owner);
                    Assert.Equal(WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout().ToString(), activeLock.Timeout, StringComparer.OrdinalIgnoreCase);
                    Assert.NotNull(activeLock.LockToken?.Href);
                    Assert.True(Uri.IsWellFormedUriString(activeLock.LockToken.Href, UriKind.RelativeOrAbsolute));
                });
        }

        [Fact]
        public async Task AddLockToRootRecursiveWithTimeoutTest()
        {
            var response = await Client.LockAsync(
                "/",
                WebDavTimeoutHeaderValue.CreateWebDavTimeout(TimeSpan.FromSeconds(1)),
                WebDavDepthHeaderValue.Infinity,
                new LockInfo()
                {
                    LockScope = LockScope.CreateExclusiveLockScope(),
                    LockType = LockType.CreateWriteLockType(),
                }).ConfigureAwait(false);
            var prop = await response.EnsureSuccessStatusCode()
                .Content.ParsePropResponseContentAsync().ConfigureAwait(false);
            Assert.Collection(
                prop.LockDiscovery.ActiveLock,
                activeLock =>
                {
                    Assert.Equal("/", activeLock.LockRoot.Href);
                    Assert.Equal(WebDavDepthHeaderValue.Infinity.ToString(), activeLock.Depth, StringComparer.OrdinalIgnoreCase);
                    Assert.IsType<Exclusive>(activeLock.LockScope.Item);
                    Assert.Null(activeLock.Owner);
                    Assert.Equal(WebDavTimeoutHeaderValue.CreateWebDavTimeout(TimeSpan.FromSeconds(1)).ToString(), activeLock.Timeout, StringComparer.OrdinalIgnoreCase);
                    Assert.NotNull(activeLock.LockToken?.Href);
                    Assert.True(Uri.IsWellFormedUriString(activeLock.LockToken.Href, UriKind.RelativeOrAbsolute));
                });
        }

        [Fact]
        public async Task AddLockToRootRecursiveWithPrincipalOwnerTest()
        {
            var response = await Client.LockAsync(
                    "/",
                    WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout(),
                    WebDavDepthHeaderValue.Infinity,
                    new LockInfo()
                    {
                        LockScope = LockScope.CreateExclusiveLockScope(),
                        LockType = LockType.CreateWriteLockType(),
                        Owner = new XElement("{DAV:}owner", "principal"),
                    })
                .ConfigureAwait(false);
            var prop = await response.EnsureSuccessStatusCode()
                .Content.ParsePropResponseContentAsync().ConfigureAwait(false);
            Assert.Collection(
                prop.LockDiscovery.ActiveLock,
                activeLock =>
                {
                    Assert.Equal("/", activeLock.LockRoot.Href);
                    Assert.Equal(WebDavDepthHeaderValue.Infinity.ToString(), activeLock.Depth, StringComparer.OrdinalIgnoreCase);
                    Assert.IsType<Exclusive>(activeLock.LockScope.Item);
                    Assert.Equal("<owner xmlns=\"DAV:\">principal</owner>", activeLock.Owner.ToString(SaveOptions.DisableFormatting));
                    Assert.Equal(WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout().ToString(), activeLock.Timeout, StringComparer.OrdinalIgnoreCase);
                    Assert.NotNull(activeLock.LockToken?.Href);
                    Assert.True(Uri.IsWellFormedUriString(activeLock.LockToken.Href, UriKind.RelativeOrAbsolute));
                });
        }

        [Fact]
        public async Task AddLockToRootRecursiveWithUriOwnerTest()
        {
            var response = await Client.LockAsync(
                    "/",
                    WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout(),
                    WebDavDepthHeaderValue.Infinity,
                    new LockInfo()
                    {
                        LockScope = LockScope.CreateExclusiveLockScope(),
                        LockType = LockType.CreateWriteLockType(),
                        Owner = new XElement("{DAV:}owner", new XElement("{DAV:}href", "http://localhost/uri-owner")),
                    })
                .ConfigureAwait(false);
            var prop = await response.EnsureSuccessStatusCode()
                .Content.ParsePropResponseContentAsync().ConfigureAwait(false);
            Assert.Collection(
                prop.LockDiscovery.ActiveLock,
                activeLock =>
                {
                    Assert.Equal("/", activeLock.LockRoot.Href);
                    Assert.Equal(WebDavDepthHeaderValue.Infinity.ToString(), activeLock.Depth, StringComparer.OrdinalIgnoreCase);
                    Assert.IsType<Exclusive>(activeLock.LockScope.Item);
                    Assert.Equal("<owner xmlns=\"DAV:\"><href>http://localhost/uri-owner</href></owner>", activeLock.Owner.ToString(SaveOptions.DisableFormatting));
                    Assert.Equal(WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout().ToString(), activeLock.Timeout, StringComparer.OrdinalIgnoreCase);
                    Assert.NotNull(activeLock.LockToken?.Href);
                    Assert.True(Uri.IsWellFormedUriString(activeLock.LockToken.Href, UriKind.RelativeOrAbsolute));
                });
        }

        [Fact]
        public async Task AddLockToRootRecursiveWithAttributeOwnerTest()
        {
            var response = await Client.LockAsync(
                    "/",
                    WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout(),
                    WebDavDepthHeaderValue.Infinity,
                    new LockInfo()
                    {
                        LockScope = LockScope.CreateExclusiveLockScope(),
                        LockType = LockType.CreateWriteLockType(),
                        Owner = new XElement("{DAV:}owner", new XAttribute("attr", "attr-value")),
                    })
                .ConfigureAwait(false);
            var prop = await response.EnsureSuccessStatusCode()
                .Content.ParsePropResponseContentAsync().ConfigureAwait(false);
            Assert.Collection(
                prop.LockDiscovery.ActiveLock,
                activeLock =>
                {
                    Assert.Equal("/", activeLock.LockRoot.Href);
                    Assert.Equal(WebDavDepthHeaderValue.Infinity.ToString(), activeLock.Depth, StringComparer.OrdinalIgnoreCase);
                    Assert.IsType<Exclusive>(activeLock.LockScope.Item);
                    Assert.Equal("<owner attr=\"attr-value\" xmlns=\"DAV:\" />", activeLock.Owner.ToString(SaveOptions.DisableFormatting));
                    Assert.Equal(WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout().ToString(), activeLock.Timeout, StringComparer.OrdinalIgnoreCase);
                    Assert.NotNull(activeLock.LockToken?.Href);
                    Assert.True(Uri.IsWellFormedUriString(activeLock.LockToken.Href, UriKind.RelativeOrAbsolute));
                });
        }
    }
}
