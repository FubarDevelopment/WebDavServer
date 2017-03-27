// <copyright file="RangeTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.Model.Headers;

using Xunit;

namespace FubarDev.WebDavServer.Tests.ModelTests
{
    public class RangeTests
    {
        [Fact]
        public void FromToTest()
        {
            var range = RangeHeader.Parse("bytes=0-499");
            var rangeItems = range.Normalize(10000);
            Assert.Collection(
                rangeItems,
                rangeItem =>
                {
                    Assert.Equal(0, rangeItem.From);
                    Assert.Equal(499, rangeItem.To);
                });
        }

        [Fact]
        public void FromToTwiceTest()
        {
            var range = RangeHeader.Parse("bytes=600-999,0-499");
            var rangeItems = range.Normalize(10000);
            Assert.Collection(
                rangeItems,
                rangeItem =>
                {
                    Assert.Equal(0, rangeItem.From);
                    Assert.Equal(499, rangeItem.To);
                },
                rangeItem =>
                {
                    Assert.Equal(600, rangeItem.From);
                    Assert.Equal(999, rangeItem.To);
                });
        }

        [Fact]
        public void FromTest()
        {
            var range = RangeHeader.Parse("bytes=600-");
            var rangeItems = range.Normalize(10000);
            Assert.Collection(
                rangeItems,
                rangeItem =>
                {
                    Assert.Equal(600, rangeItem.From);
                    Assert.Equal(9999, rangeItem.To);
                });
        }

        [Fact]
        public void ToTest()
        {
            var range = RangeHeader.Parse("bytes=-5000");
            var rangeItems = range.Normalize(10000);
            Assert.Collection(
                rangeItems,
                rangeItem =>
                {
                    Assert.Equal(5000, rangeItem.From);
                    Assert.Equal(9999, rangeItem.To);
                });
        }

        [Fact]
        public void OverlapAscendingTest()
        {
            var range = RangeHeader.Parse("bytes=0-499,300-599");
            var rangeItems = range.Normalize(10000);
            Assert.Collection(
                rangeItems,
                rangeItem =>
                {
                    Assert.Equal(0, rangeItem.From);
                    Assert.Equal(599, rangeItem.To);
                });
        }

        [Fact]
        public void OverlapDescendingTest()
        {
            var range = RangeHeader.Parse("bytes=300-599,0-499");
            var rangeItems = range.Normalize(10000);
            Assert.Collection(
                rangeItems,
                rangeItem =>
                {
                    Assert.Equal(0, rangeItem.From);
                    Assert.Equal(599, rangeItem.To);
                });
        }

        [Fact]
        public void OverlapWithToTest()
        {
            var range = RangeHeader.Parse("bytes=4000-5999,-5000");
            var rangeItems = range.Normalize(10000);
            Assert.Collection(
                rangeItems,
                rangeItem =>
                {
                    Assert.Equal(4000, rangeItem.From);
                    Assert.Equal(9999, rangeItem.To);
                });
        }

        [Fact]
        public void OverlapWithFromTest()
        {
            var range = RangeHeader.Parse("bytes=4000-5999,5000-");
            var rangeItems = range.Normalize(10000);
            Assert.Collection(
                rangeItems,
                rangeItem =>
                {
                    Assert.Equal(4000, rangeItem.From);
                    Assert.Equal(9999, rangeItem.To);
                });
        }

        [Fact]
        public void OverlapDescendingWithFromTest()
        {
            var range = RangeHeader.Parse("bytes=5000-,4000-5999");
            var rangeItems = range.Normalize(10000);
            Assert.Collection(
                rangeItems,
                rangeItem =>
                {
                    Assert.Equal(4000, rangeItem.From);
                    Assert.Equal(9999, rangeItem.To);
                });
        }
    }
}
