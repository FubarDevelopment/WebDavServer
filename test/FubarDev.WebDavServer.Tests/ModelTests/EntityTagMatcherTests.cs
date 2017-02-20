// <copyright file="EntityTagMatcherTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

using FubarDev.WebDavServer.Model.Headers;

using Xunit;

namespace FubarDev.WebDavServer.Tests.ModelTests
{
    public class EntityTagMatcherTests
    {
        private static readonly IReadOnlyCollection<EntityTag> _entityTags = EntityTag.Parse("\"qwe\", w/\"qwe\", \"asd\"").ToList();

        [Fact]
        public void IfMatchAllNullTest()
        {
            var matcher = IfMatchHeader.Parse((string)null);
            Assert.All(_entityTags, etag => Assert.True(matcher.IsMatch(etag)));
        }

        [Fact]
        public void IfMatchAllEmptyTest()
        {
            var matcher = IfMatchHeader.Parse(string.Empty);
            Assert.All(_entityTags, etag => Assert.True(matcher.IsMatch(etag)));
        }

        [Fact]
        public void IfMatchAllStarTest()
        {
            var matcher = IfMatchHeader.Parse("*");
            Assert.All(_entityTags, etag => Assert.True(matcher.IsMatch(etag)));
        }

        [Fact]
        public void IfMatchStrongTest()
        {
            var matcher = IfMatchHeader.Parse("\"qwe\"");
            Assert.Equal(1, _entityTags.Count(etag => matcher.IsMatch(etag)));
        }

        [Fact]
        public void IfMatchWeakWithStrongCompareTest()
        {
            var matcher = IfMatchHeader.Parse("w/\"qwe\"");
            Assert.Equal(0, _entityTags.Count(etag => matcher.IsMatch(etag)));
        }

        [Fact]
        public void IfMatchWeakWithWeakCompareTest()
        {
            var matcher = IfMatchHeader.Parse("w/\"qwe\"", EntityTagComparer.Weak);
            Assert.Equal(2, _entityTags.Count(etag => matcher.IsMatch(etag)));
        }

        [Fact]
        public void IfMatchOtherTest()
        {
            var matcher = IfMatchHeader.Parse("\"asd\"");
            Assert.Equal(1, _entityTags.Count(etag => matcher.IsMatch(etag)));
        }

        [Fact]
        public void IfMatchNoneTest()
        {
            var matcher = IfMatchHeader.Parse("\"qweqwe\"");
            Assert.Equal(0, _entityTags.Count(etag => matcher.IsMatch(etag)));
        }

        [Fact]
        public void IfNoneMatchAllNullTest()
        {
            var matcher = IfNoneMatchHeader.Parse((string)null);
            Assert.All(_entityTags, etag => Assert.False(matcher.IsMatch(etag)));
        }

        [Fact]
        public void IfNoneMatchAllEmptyTest()
        {
            var matcher = IfNoneMatchHeader.Parse(string.Empty);
            Assert.All(_entityTags, etag => Assert.False(matcher.IsMatch(etag)));
        }

        [Fact]
        public void IfNoneMatchAllStarTest()
        {
            var matcher = IfNoneMatchHeader.Parse("*");
            Assert.All(_entityTags, etag => Assert.False(matcher.IsMatch(etag)));
        }

        [Fact]
        public void IfNoneMatchStrongWithStrongComparerTest()
        {
            var matcher = IfNoneMatchHeader.Parse("\"qwe\"");
            Assert.Equal(2, _entityTags.Count(etag => matcher.IsMatch(etag)));
        }

        [Fact]
        public void IfNoneMatchStrongTest()
        {
            var matcher = IfNoneMatchHeader.Parse("\"qwe\"", EntityTagComparer.Weak);
            Assert.Equal(1, _entityTags.Count(etag => matcher.IsMatch(etag)));
        }

        [Fact]
        public void IfNoneMatchWeakWithStrongCompareTest()
        {
            var matcher = IfNoneMatchHeader.Parse("w/\"qwe\"");
            Assert.Equal(3, _entityTags.Count(etag => matcher.IsMatch(etag)));
        }

        [Fact]
        public void IfNoneMatchWeakTest()
        {
            var matcher = IfNoneMatchHeader.Parse("w/\"qwe\"", EntityTagComparer.Weak);
            Assert.Equal(1, _entityTags.Count(etag => matcher.IsMatch(etag)));
        }

        [Fact]
        public void IfNoneMatchOtherTest()
        {
            var matcher = IfNoneMatchHeader.Parse("\"asd\"");
            Assert.Equal(2, _entityTags.Count(etag => matcher.IsMatch(etag)));
        }

        [Fact]
        public void IfNoneMatchNoneTest()
        {
            var matcher = IfNoneMatchHeader.Parse("\"qweqwe\"");
            Assert.Equal(3, _entityTags.Count(etag => matcher.IsMatch(etag)));
        }
    }
}
