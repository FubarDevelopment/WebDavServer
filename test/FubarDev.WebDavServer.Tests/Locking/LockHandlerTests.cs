// <copyright file="LockHandlerTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using DecaTec.WebDav;
using DecaTec.WebDav.Headers;
using DecaTec.WebDav.WebDavArtifacts;

using FubarDev.WebDavServer.Model.Headers;

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
            var prop = await WebDavResponseContentParser.ParsePropResponseContentAsync(response.EnsureSuccessStatusCode().Content).ConfigureAwait(false);
            var lockToken = CodedUrlParser.Parse(response.Headers.GetValues(WebDavRequestHeader.LockToken).Single());
            Assert.NotNull(prop.LockDiscovery);
            Assert.Collection(
                prop.LockDiscovery.ActiveLock,
                activeLock =>
                {
                    Assert.Equal("/", activeLock.LockRoot.Href);
                    Assert.Equal(WebDavDepthHeaderValue.Infinity.ToString(), activeLock.Depth, StringComparer.OrdinalIgnoreCase);
                    Assert.IsType<Exclusive>(activeLock.LockScope.Item);
                    Assert.Null(activeLock.OwnerRaw);
                    Assert.Equal(WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout().ToString(), activeLock.Timeout, StringComparer.OrdinalIgnoreCase);
                    Assert.NotNull(activeLock.LockToken?.Href);
                    Assert.True(Uri.IsWellFormedUriString(activeLock.LockToken!.Href!, UriKind.RelativeOrAbsolute));
                    Assert.Equal(lockToken.OriginalString, activeLock.LockToken.Href);
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
            var prop = await WebDavResponseContentParser.ParsePropResponseContentAsync(response.EnsureSuccessStatusCode().Content).ConfigureAwait(false);
            Assert.Collection(
                prop.LockDiscovery.ActiveLock,
                activeLock =>
                {
                    Assert.Equal("/", activeLock.LockRoot.Href);
                    Assert.Equal(WebDavDepthHeaderValue.Infinity.ToString(), activeLock.Depth, StringComparer.OrdinalIgnoreCase);
                    Assert.IsType<Exclusive>(activeLock.LockScope.Item);
                    Assert.Null(activeLock.OwnerRaw);
                    Assert.Equal(WebDavTimeoutHeaderValue.CreateWebDavTimeout(TimeSpan.FromSeconds(1)).ToString(), activeLock.Timeout, StringComparer.OrdinalIgnoreCase);
                    Assert.NotNull(activeLock.LockToken?.Href);
                    Assert.True(Uri.IsWellFormedUriString(activeLock.LockToken!.Href!, UriKind.RelativeOrAbsolute));
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
                        OwnerRaw = new XElement("{DAV:}owner", "principal"),
                    })
                .ConfigureAwait(false);
            var prop = await WebDavResponseContentParser.ParsePropResponseContentAsync(response.EnsureSuccessStatusCode().Content).ConfigureAwait(false);
            Assert.Collection(
                prop.LockDiscovery.ActiveLock,
                activeLock =>
                {
                    Assert.Equal("/", activeLock.LockRoot.Href);
                    Assert.Equal(WebDavDepthHeaderValue.Infinity.ToString(), activeLock.Depth, StringComparer.OrdinalIgnoreCase);
                    Assert.IsType<Exclusive>(activeLock.LockScope.Item);
                    Assert.Equal("<owner xmlns=\"DAV:\">principal</owner>", activeLock.OwnerRaw.ToString(SaveOptions.DisableFormatting));
                    Assert.Equal(WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout().ToString(), activeLock.Timeout, StringComparer.OrdinalIgnoreCase);
                    Assert.NotNull(activeLock.LockToken?.Href);
                    Assert.True(Uri.IsWellFormedUriString(activeLock.LockToken!.Href!, UriKind.RelativeOrAbsolute));
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
                        OwnerRaw = new XElement("{DAV:}owner", new XElement("{DAV:}href", "http://localhost/uri-owner")),
                    })
                .ConfigureAwait(false);
            var prop = await WebDavResponseContentParser.ParsePropResponseContentAsync(response.EnsureSuccessStatusCode().Content).ConfigureAwait(false);
            Assert.Collection(
                prop.LockDiscovery.ActiveLock,
                activeLock =>
                {
                    Assert.Equal("/", activeLock.LockRoot.Href);
                    Assert.Equal(WebDavDepthHeaderValue.Infinity.ToString(), activeLock.Depth, StringComparer.OrdinalIgnoreCase);
                    Assert.IsType<Exclusive>(activeLock.LockScope.Item);
                    Assert.Equal("<owner xmlns=\"DAV:\"><href>http://localhost/uri-owner</href></owner>", activeLock.OwnerRaw.ToString(SaveOptions.DisableFormatting));
                    Assert.Equal(WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout().ToString(), activeLock.Timeout, StringComparer.OrdinalIgnoreCase);
                    Assert.NotNull(activeLock.LockToken?.Href);
                    Assert.True(Uri.IsWellFormedUriString(activeLock.LockToken!.Href!, UriKind.RelativeOrAbsolute));
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
                        OwnerRaw = new XElement("{DAV:}owner", new XAttribute("attr", "attr-value")),
                    })
                .ConfigureAwait(false);
            var prop = await WebDavResponseContentParser.ParsePropResponseContentAsync(response.EnsureSuccessStatusCode().Content).ConfigureAwait(false);
            Assert.NotNull(prop?.LockDiscovery?.ActiveLock);
            Assert.Collection(
                prop!.LockDiscovery!.ActiveLock!,
                activeLock =>
                {
                    Assert.Equal("/", activeLock.LockRoot.Href);
                    Assert.Equal(
                        WebDavDepthHeaderValue.Infinity.ToString(),
                        activeLock.Depth,
                        StringComparer.OrdinalIgnoreCase);
                    Assert.IsType<Exclusive>(activeLock.LockScope.Item);
                    Assert.Equal(
                        "<owner attr=\"attr-value\" xmlns=\"DAV:\" />",
                        activeLock.OwnerRaw.ToString(SaveOptions.DisableFormatting));
                    Assert.Equal(
                        WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout().ToString(),
                        activeLock.Timeout,
                        StringComparer.OrdinalIgnoreCase);
                    Assert.NotNull(activeLock.LockToken?.Href);
                    Assert.True(Uri.IsWellFormedUriString(activeLock.LockToken!.Href!, UriKind.RelativeOrAbsolute));
                });
        }

        [Fact]
        public async Task AddLockToRootAndQueryLockDiscoveryTest()
        {
            var lockResponse = await Client.LockAsync(
                    "/",
                    WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout(),
                    WebDavDepthHeaderValue.Infinity,
                    new LockInfo()
                    {
                        LockScope = LockScope.CreateExclusiveLockScope(),
                        LockType = LockType.CreateWriteLockType(),
                    })
                .ConfigureAwait(false);
            lockResponse.EnsureSuccessStatusCode();
            var propFindResponse = await Client.PropFindAsync(
                "/",
                WebDavDepthHeaderValue.Zero,
                new PropFind()
                {
                    Item = new Prop()
                    {
                        LockDiscovery = new LockDiscovery(),
                    },
                }).ConfigureAwait(false);
            Assert.Equal(WebDavStatusCode.MultiStatus, propFindResponse.StatusCode);
            var multiStatus = await WebDavResponseContentParser.ParseMultistatusResponseContentAsync(propFindResponse.Content).ConfigureAwait(false);
            Assert.Collection(
                multiStatus.Response,
                response =>
                {
                    Assert.Equal("/", response.Href);
                    Assert.Collection(
                        response.ItemsElementName,
                        n => Assert.Equal(ItemsChoiceType.propstat, n));
                    Assert.Collection(
                        response.Items,
                        item =>
                        {
                            var propStat = Assert.IsType<Propstat>(item);
                            var status = Model.Status.Parse(propStat.Status);
                            Assert.Equal(200, status.StatusCode);
                            Assert.NotNull(propStat.Prop?.LockDiscovery?.ActiveLock);
                            Assert.Collection(
                                propStat.Prop!.LockDiscovery!.ActiveLock!,
                                activeLock =>
                                {
                                    Assert.Equal("/", activeLock.LockRoot.Href);
                                    Assert.Equal(WebDavDepthHeaderValue.Infinity.ToString(), activeLock.Depth, StringComparer.OrdinalIgnoreCase);
                                    Assert.IsType<Exclusive>(activeLock.LockScope.Item);
                                    Assert.Null(activeLock.OwnerRaw);
                                    Assert.Equal(WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout().ToString(), activeLock.Timeout, StringComparer.OrdinalIgnoreCase);
                                    Assert.NotNull(activeLock.LockToken?.Href);
                                    Assert.True(Uri.IsWellFormedUriString(activeLock.LockToken!.Href!, UriKind.RelativeOrAbsolute));
                                });
                        });
                });
        }

        [Fact]
        public async Task AddLockToRootAndTryRefreshWrongLockTest()
        {
            var lockResponse = await Client.LockAsync(
                    "/",
                    WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout(),
                    WebDavDepthHeaderValue.Infinity,
                    new LockInfo()
                    {
                        LockScope = LockScope.CreateExclusiveLockScope(),
                        LockType = LockType.CreateWriteLockType(),
                    })
                .ConfigureAwait(false);
            lockResponse.EnsureSuccessStatusCode();
            Assert.True(AbsoluteUri.TryParse("urn:asasdasd", out var lockTokenUri));
            var refreshResult = await Client.RefreshLockAsync(
                "/",
                WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout(),
                new LockToken(lockTokenUri)).ConfigureAwait(false);
            Assert.Equal(WebDavStatusCode.PreconditionFailed, refreshResult.StatusCode);
        }

        [Fact]
        public async Task AddLockToRootAndTryRefreshTest()
        {
            var lockResponse = await Client.LockAsync(
                    "/",
                    WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout(),
                    WebDavDepthHeaderValue.Infinity,
                    new LockInfo()
                    {
                        LockScope = LockScope.CreateExclusiveLockScope(),
                        LockType = LockType.CreateWriteLockType(),
                    })
                .ConfigureAwait(false);
            lockResponse.EnsureSuccessStatusCode();
            var prop = await WebDavResponseContentParser.ParsePropResponseContentAsync(lockResponse.Content).ConfigureAwait(false);
            var lockToken = prop.LockDiscovery.ActiveLock.Single().LockToken;
            Assert.True(AbsoluteUri.TryParse(lockToken.Href, out var lockTokenUri));
            var refreshResponse = await Client.RefreshLockAsync(
                "/",
                WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout(),
                new LockToken(lockTokenUri)).ConfigureAwait(false);
            refreshResponse.EnsureSuccessStatusCode();
            var refreshProp = await WebDavResponseContentParser.ParsePropResponseContentAsync(refreshResponse.Content).ConfigureAwait(false);
            Assert.NotNull(refreshProp.LockDiscovery);
        }

        [Fact]
        public async Task TryRelockRefreshedLock()
        {
            var lockResponse = await Client.LockAsync(
                    "/",
                    WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout(),
                    WebDavDepthHeaderValue.Infinity,
                    new LockInfo()
                    {
                        LockScope = LockScope.CreateExclusiveLockScope(),
                        LockType = LockType.CreateWriteLockType(),
                    })
                .ConfigureAwait(false);
            lockResponse.EnsureSuccessStatusCode();
            var prop = await WebDavResponseContentParser.ParsePropResponseContentAsync(lockResponse.Content).ConfigureAwait(false);
            var lockToken = prop.LockDiscovery.ActiveLock.Single().LockToken;
            Assert.True(AbsoluteUri.TryParse(lockToken.Href, out var lockTokenUri));
            var refreshResponse = await Client.RefreshLockAsync(
                "/",
                WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout(),
                new LockToken(lockTokenUri)).ConfigureAwait(false);
            refreshResponse.EnsureSuccessStatusCode();
            var refreshProp = await WebDavResponseContentParser.ParsePropResponseContentAsync(refreshResponse.Content).ConfigureAwait(false);
            Assert.NotNull(refreshProp.LockDiscovery);
            var refreshLockToken = refreshProp.LockDiscovery.ActiveLock.Single().LockToken;
            Assert.Equal(lockToken.Href, refreshLockToken.Href);

            Assert.True(AbsoluteUri.TryParse(refreshLockToken.Href, out var refreshLockTokenUri));
            var unlockResponse = await Client.UnlockAsync("/", new LockToken(refreshLockTokenUri)).ConfigureAwait(false);
            unlockResponse.EnsureSuccessStatusCode();
            var secondLockResponse = await Client.LockAsync(
                    "/",
                    WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout(),
                    WebDavDepthHeaderValue.Infinity,
                    new LockInfo()
                    {
                        LockScope = LockScope.CreateExclusiveLockScope(),
                        LockType = LockType.CreateWriteLockType(),
                    })
                .ConfigureAwait(false);
            secondLockResponse.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task AddLockCreatesDocumentTest()
        {
            var response = await Client.LockAsync(
                "/test1.txt",
                WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout(),
                WebDavDepthHeaderValue.Zero,
                new LockInfo()
                {
                    LockScope = LockScope.CreateExclusiveLockScope(),
                    LockType = LockType.CreateWriteLockType(),
                }).ConfigureAwait(false);
            var prop = await WebDavResponseContentParser.ParsePropResponseContentAsync(response.EnsureSuccessStatusCode().Content).ConfigureAwait(false);
            Assert.NotNull(prop.LockDiscovery);
            Assert.Collection(
                prop.LockDiscovery.ActiveLock,
                activeLock =>
                {
                    Assert.Equal("/test1.txt", activeLock.LockRoot.Href);
                    Assert.Equal(WebDavDepthHeaderValue.Zero.ToString(), activeLock.Depth, StringComparer.OrdinalIgnoreCase);
                    Assert.IsType<Exclusive>(activeLock.LockScope.Item);
                    Assert.Null(activeLock.OwnerRaw);
                    Assert.Equal(WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout().ToString(), activeLock.Timeout, StringComparer.OrdinalIgnoreCase);
                    Assert.NotNull(activeLock.LockToken?.Href);
                    Assert.True(Uri.IsWellFormedUriString(activeLock.LockToken!.Href!, UriKind.RelativeOrAbsolute));
                });

            var ct = CancellationToken.None;
            var fileSystem = GetFileSystem();
            var root = await fileSystem.Root;
            var doc = await root.GetChildAsync("test1.txt", ct).ConfigureAwait(false);
            Assert.NotNull(doc);
        }
    }
}
