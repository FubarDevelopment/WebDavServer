// <copyright file="EntityTagMatcherTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using FubarDev.WebDavServer.Model;

using Xunit;

namespace FubarDev.WebDavServer.Tests.ModelTests
{
    public class EntityTagMatcherTests
    {
        private static readonly IReadOnlyCollection<EntityTag> _entityTags = EntityTag.Parse("\"qwe\", w/\"qwe\", \"asd\"").ToList();
        private static readonly IReadOnlyCollection<Uri> _stateTokens = new List<Uri>();

        [Fact]
        public void IfMatchAllNullTest()
        {
            var matcher = IfMatch.Parse(null);
            Assert.All(_entityTags, etag => Assert.True(matcher.IsMatch(etag, _stateTokens)));
        }

        [Fact]
        public void IfMatchAllEmptyTest()
        {
            var matcher = IfMatch.Parse(string.Empty);
            Assert.All(_entityTags, etag => Assert.True(matcher.IsMatch(etag, _stateTokens)));
        }

        [Fact]
        public void IfMatchAllStarTest()
        {
            var matcher = IfMatch.Parse("*");
            Assert.All(_entityTags, etag => Assert.True(matcher.IsMatch(etag, _stateTokens)));
        }

        [Fact]
        public void IfMatchStrongTest()
        {
            var matcher = IfMatch.Parse("\"qwe\"");
            Assert.Equal(1, _entityTags.Count(etag => matcher.IsMatch(etag, _stateTokens)));
        }

        [Fact]
        public void IfMatchWeakTest()
        {
            var matcher = IfMatch.Parse("w/\"qwe\"");
            Assert.Equal(1, _entityTags.Count(etag => matcher.IsMatch(etag, _stateTokens)));
        }

        [Fact]
        public void IfMatchOtherTest()
        {
            var matcher = IfMatch.Parse("\"asd\"");
            Assert.Equal(1, _entityTags.Count(etag => matcher.IsMatch(etag, _stateTokens)));
        }

        [Fact]
        public void IfMatchNoneTest()
        {
            var matcher = IfMatch.Parse("\"qweqwe\"");
            Assert.Equal(0, _entityTags.Count(etag => matcher.IsMatch(etag, _stateTokens)));
        }

        [Fact]
        public void IfNoneMatchAllNullTest()
        {
            var matcher = IfNoneMatch.Parse(null);
            Assert.All(_entityTags, etag => Assert.False(matcher.IsMatch(etag, _stateTokens)));
        }

        [Fact]
        public void IfNoneMatchAllEmptyTest()
        {
            var matcher = IfNoneMatch.Parse(string.Empty);
            Assert.All(_entityTags, etag => Assert.False(matcher.IsMatch(etag, _stateTokens)));
        }

        [Fact]
        public void IfNoneMatchAllStarTest()
        {
            var matcher = IfNoneMatch.Parse("*");
            Assert.All(_entityTags, etag => Assert.False(matcher.IsMatch(etag, _stateTokens)));
        }

        [Fact]
        public void IfNoneMatchStrongTest()
        {
            var matcher = IfNoneMatch.Parse("\"qwe\"");
            Assert.Equal(2, _entityTags.Count(etag => matcher.IsMatch(etag, _stateTokens)));
        }

        [Fact]
        public void IfNoneMatchWeakTest()
        {
            var matcher = IfNoneMatch.Parse("w/\"qwe\"");
            Assert.Equal(2, _entityTags.Count(etag => matcher.IsMatch(etag, _stateTokens)));
        }

        [Fact]
        public void IfNoneMatchOtherTest()
        {
            var matcher = IfNoneMatch.Parse("\"asd\"");
            Assert.Equal(2, _entityTags.Count(etag => matcher.IsMatch(etag, _stateTokens)));
        }

        [Fact]
        public void IfNoneMatchNoneTest()
        {
            var matcher = IfNoneMatch.Parse("\"qweqwe\"");
            Assert.Equal(3, _entityTags.Count(etag => matcher.IsMatch(etag, _stateTokens)));
        }
    }
}
