// <copyright file="EntityTagMatcherTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.InMemory;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Store.InMemory;

using Xunit;

namespace FubarDev.WebDavServer.Tests.ModelTests
{
    public class EntityTagMatcherTests
    {
        private static readonly IReadOnlyCollection<EntityTag> _entityTags = EntityTag.Parse("\"qwe\", w/\"qwe\", \"asd\"").ToList();
        private static readonly IReadOnlyCollection<Uri> _stateTokens = new List<Uri>();

        public EntityTagMatcherTests()
        {
            FileSystem = new InMemoryFileSystem(
                new PathTraversalEngine(),
                new SystemClock(),
                new DeadPropertyFactory(),
                new InMemoryPropertyStoreFactory());
        }

        public IFileSystem FileSystem { get; }

        [Fact]
        public async Task IfMatchAllNullTest()
        {
            var root = await FileSystem.Root;
            var matcher = IfMatch.Parse((string)null);
            Assert.All(_entityTags, etag => Assert.True(matcher.IsMatch(root, etag, _stateTokens)));
        }

        [Fact]
        public async Task IfMatchAllEmptyTest()
        {
            var root = await FileSystem.Root;
            var matcher = IfMatch.Parse(string.Empty);
            Assert.All(_entityTags, etag => Assert.True(matcher.IsMatch(root, etag, _stateTokens)));
        }

        [Fact]
        public async Task IfMatchAllStarTest()
        {
            var root = await FileSystem.Root;
            var matcher = IfMatch.Parse("*");
            Assert.All(_entityTags, etag => Assert.True(matcher.IsMatch(root, etag, _stateTokens)));
        }

        [Fact]
        public async Task IfMatchStrongTest()
        {
            var root = await FileSystem.Root;
            var matcher = IfMatch.Parse("\"qwe\"");
            Assert.Equal(1, _entityTags.Count(etag => matcher.IsMatch(root, etag, _stateTokens)));
        }

        [Fact]
        public async Task IfMatchWeakTest()
        {
            var root = await FileSystem.Root;
            var matcher = IfMatch.Parse("w/\"qwe\"");
            Assert.Equal(1, _entityTags.Count(etag => matcher.IsMatch(root, etag, _stateTokens)));
        }

        [Fact]
        public async Task IfMatchOtherTest()
        {
            var root = await FileSystem.Root;
            var matcher = IfMatch.Parse("\"asd\"");
            Assert.Equal(1, _entityTags.Count(etag => matcher.IsMatch(root, etag, _stateTokens)));
        }

        [Fact]
        public async Task IfMatchNoneTest()
        {
            var root = await FileSystem.Root;
            var matcher = IfMatch.Parse("\"qweqwe\"");
            Assert.Equal(0, _entityTags.Count(etag => matcher.IsMatch(root, etag, _stateTokens)));
        }

        [Fact]
        public async Task IfNoneMatchAllNullTest()
        {
            var root = await FileSystem.Root;
            var matcher = IfNoneMatch.Parse((string)null);
            Assert.All(_entityTags, etag => Assert.False(matcher.IsMatch(root, etag, _stateTokens)));
        }

        [Fact]
        public async Task IfNoneMatchAllEmptyTest()
        {
            var root = await FileSystem.Root;
            var matcher = IfNoneMatch.Parse(string.Empty);
            Assert.All(_entityTags, etag => Assert.False(matcher.IsMatch(root, etag, _stateTokens)));
        }

        [Fact]
        public async Task IfNoneMatchAllStarTest()
        {
            var root = await FileSystem.Root;
            var matcher = IfNoneMatch.Parse("*");
            Assert.All(_entityTags, etag => Assert.False(matcher.IsMatch(root, etag, _stateTokens)));
        }

        [Fact]
        public async Task IfNoneMatchStrongTest()
        {
            var root = await FileSystem.Root;
            var matcher = IfNoneMatch.Parse("\"qwe\"");
            Assert.Equal(2, _entityTags.Count(etag => matcher.IsMatch(root, etag, _stateTokens)));
        }

        [Fact]
        public async Task IfNoneMatchWeakTest()
        {
            var root = await FileSystem.Root;
            var matcher = IfNoneMatch.Parse("w/\"qwe\"");
            Assert.Equal(2, _entityTags.Count(etag => matcher.IsMatch(root, etag, _stateTokens)));
        }

        [Fact]
        public async Task IfNoneMatchOtherTest()
        {
            var root = await FileSystem.Root;
            var matcher = IfNoneMatch.Parse("\"asd\"");
            Assert.Equal(2, _entityTags.Count(etag => matcher.IsMatch(root, etag, _stateTokens)));
        }

        [Fact]
        public async Task IfNoneMatchNoneTest()
        {
            var root = await FileSystem.Root;
            var matcher = IfNoneMatch.Parse("\"qweqwe\"");
            Assert.Equal(3, _entityTags.Count(etag => matcher.IsMatch(root, etag, _stateTokens)));
        }
    }
}
