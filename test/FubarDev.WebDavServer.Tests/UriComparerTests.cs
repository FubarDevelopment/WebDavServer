// <copyright file="UriComparerTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Security.Principal;

using FubarDev.WebDavServer.Utils;
using FubarDev.WebDavServer.Utils.UAParser;

using Xunit;

namespace FubarDev.WebDavServer.Tests
{
    public class UriComparerTests
    {
        [Fact]
        public void CompareRelativeToAbsoluteFile()
        {
            var comparer = new DefaultUriComparer(new TestWebDavContextAccessor(new Uri("http://localhost/")));
            var compareResult = comparer.Compare(
                new Uri("/test", UriKind.Relative),
                new Uri("/test/a"));
            Assert.Equal(UriComparisonResult.Parent, compareResult);
        }

        [Theory]
        [InlineData("http://localhost/")]
        [InlineData("//localhost/")]
        [InlineData("http://localhost:80/")]
        [InlineData("http://a@localhost/")]
        [InlineData("http://a:b@localhost/")]
        [InlineData("http://localhost/a")]
        [InlineData("/a")]
        [InlineData("a")]
        public void CompareSameHosts(string otherUrl)
        {
            var comparer = new DefaultUriComparer();
            Assert.True(
                comparer.IsSameHost(new Uri("http://localhost/"), new Uri(otherUrl, UriKind.RelativeOrAbsolute)));
        }

        [Theory]
        [InlineData("https://localhost")]
        [InlineData("http://localhost:8080")]
        [InlineData("http://test")]
        public void CompareDifferentHosts(string otherUrl)
        {
            var comparer = new DefaultUriComparer();
            Assert.False(
                comparer.IsSameHost(new Uri("http://localhost/"), new Uri(otherUrl, UriKind.RelativeOrAbsolute)));
        }

        [Theory]
        [InlineData("http://localhost")]
        [InlineData("a")]
        public void CompareUrlIsThisServerWithoutContext(string url)
        {
            var comparer = new DefaultUriComparer();
            Assert.True(comparer.IsThisServer(new Uri(url, UriKind.RelativeOrAbsolute)));
        }

        [Theory]
        [InlineData("http://localhost")]
        [InlineData("http://localhost:80")]
        [InlineData("http://localhost/")]
        [InlineData("//localhost")]
        [InlineData("//localhost:80")]
        [InlineData("/a")]
        [InlineData("a")]
        public void CompareUrlIsThisServerWithContext(string url)
        {
            var comparer = new DefaultUriComparer(new TestWebDavContextAccessor(new Uri("http://localhost/")));
            Assert.True(comparer.IsThisServer(new Uri(url, UriKind.RelativeOrAbsolute)));
        }

        [Theory]
        [InlineData("http://localhost/_dav")]
        [InlineData("http://localhost:80/_dav")]
        [InlineData("//localhost/_dav")]
        [InlineData("//localhost:80/_dav")]
        [InlineData("a")]
        [InlineData("http://localhost/_dav/")]
        [InlineData("http://localhost:80/_dav/")]
        [InlineData("//localhost/_dav/")]
        [InlineData("//localhost:80/_dav/")]
        [InlineData("a/")]
        public void CompareUrlIsThisServerWithSubPath(string url)
        {
            var comparer = new DefaultUriComparer(new TestWebDavContextAccessor(new Uri("http://localhost/_dav")));
            Assert.True(comparer.IsThisServer(new Uri(url, UriKind.RelativeOrAbsolute)));
        }

        [Theory]
        [InlineData("http://localhost/_dav")]
        [InlineData("http://localhost:80/_dav")]
        [InlineData("//localhost/_dav")]
        [InlineData("//localhost:80/_dav")]
        [InlineData("a")]
        [InlineData("http://localhost/_dav/")]
        [InlineData("http://localhost:80/_dav/")]
        [InlineData("//localhost/_dav/")]
        [InlineData("//localhost:80/_dav/")]
        [InlineData("a/")]
        [InlineData("http://localhost/_dav/a")]
        [InlineData("http://localhost/_dav/a/")]
        public void CompareUrlIsThisServerWithSubPathEndingWithSlash(string url)
        {
            var comparer = new DefaultUriComparer(new TestWebDavContextAccessor(new Uri("http://localhost/_dav/")));
            Assert.True(comparer.IsThisServer(new Uri(url, UriKind.RelativeOrAbsolute)));
        }

        [Theory]
        [InlineData("https://localhost")]
        [InlineData("http://localhost:8080")]
        [InlineData("http://other-host")]
        [InlineData("//other-host")]
        [InlineData("//localhost:8080")]
        public void CompareUrlIsNotThisServer(string url)
        {
            var comparer = new DefaultUriComparer(new TestWebDavContextAccessor(new Uri("http://localhost/")));
            Assert.False(comparer.IsThisServer(new Uri(url, UriKind.RelativeOrAbsolute)));
        }

        [Theory]
        [InlineData("https://localhost")]
        [InlineData("http://localhost:8080")]
        [InlineData("http://other-host")]
        [InlineData("//other-host")]
        [InlineData("//localhost:8080")]
        [InlineData("http://localhost")]
        [InlineData("http://localhost/")]
        [InlineData("http://localhost/qwe")]
        [InlineData("http://localhost/_davX")]
        public void CompareUrlIsNotThisServerWithSubPath(string url)
        {
            var comparer = new DefaultUriComparer(new TestWebDavContextAccessor(new Uri("http://localhost/_dav")));
            Assert.False(comparer.IsThisServer(new Uri(url, UriKind.RelativeOrAbsolute)));
        }

        [Theory]
        [InlineData("http://test/a", UriComparisonResult.PrecedingDifferentHost)]
        [InlineData("//test/a", UriComparisonResult.PrecedingDifferentHost)]
        [InlineData("http://aaa/a", UriComparisonResult.FollowingDifferentHost)]
        [InlineData("//aaa/a", UriComparisonResult.FollowingDifferentHost)]
        [InlineData("/0", UriComparisonResult.PrecedingSibling)]
        [InlineData("/~", UriComparisonResult.FollowingSibling)]
        [InlineData("/_dav", UriComparisonResult.Equal)]
        [InlineData("", UriComparisonResult.Equal)]
        [InlineData("http://localhost/0", UriComparisonResult.PrecedingSibling)]
        [InlineData("http://localhost/~", UriComparisonResult.FollowingSibling)]
        [InlineData("http://localhost/_dav", UriComparisonResult.Equal)]
        [InlineData("//localhost/_dav", UriComparisonResult.Equal)]
        [InlineData("///_dav", UriComparisonResult.Equal)]
        [InlineData("http://localhost/", UriComparisonResult.Child)]
        [InlineData("//localhost/", UriComparisonResult.Child)]
        [InlineData("///", UriComparisonResult.Child)]
        [InlineData("http://localhost/_dav/a", UriComparisonResult.Parent)]
        [InlineData("//localhost/_dav/a", UriComparisonResult.Parent)]
        [InlineData("///_dav/a", UriComparisonResult.Parent)]
        public void TestCompare(string url, UriComparisonResult expected)
        {
            var baseUrl = new Uri("http://localhost/_dav");
            var comparer = new DefaultUriComparer(new TestWebDavContextAccessor(baseUrl));
            var result = comparer.Compare(baseUrl, new Uri(url, UriKind.RelativeOrAbsolute));
            Assert.Equal(expected, result);
        }

        private class TestWebDavContextAccessor : IWebDavContextAccessor
        {
            public TestWebDavContextAccessor(Uri publicBaseUrl)
            {
                WebDavContext = new TestWebDavContext(publicBaseUrl);
            }

            /// <inheritdoc />
            public IWebDavContext WebDavContext { get; }

            private class TestWebDavContext : IWebDavContext
            {
                public TestWebDavContext(Uri publicControllerUrl)
                {
                    PublicControllerUrl = publicControllerUrl;
                }

                /// <inheritdoc />
                public IServiceProvider RequestServices => throw new NotImplementedException();

                /// <inheritdoc />
                public string RequestProtocol => throw new NotImplementedException();

                /// <inheritdoc />
                public string RequestMethod => throw new NotImplementedException();

                /// <inheritdoc />
                public Uri HrefUrl => throw new NotImplementedException();

                /// <inheritdoc />
                public Uri PublicRelativeRequestUrl => throw new NotImplementedException();

                /// <inheritdoc />
                public Uri PublicAbsoluteRequestUrl => throw new NotImplementedException();

                /// <inheritdoc />
                public Uri PublicControllerUrl { get; }

                /// <inheritdoc />
                public Uri PublicBaseUrl => throw new NotImplementedException();

                /// <inheritdoc />
                public Uri PublicRootUrl => throw new NotImplementedException();

                /// <inheritdoc />
                public Uri ServiceRelativeRequestUrl => throw new NotImplementedException();

                /// <inheritdoc />
                public Uri ServiceAbsoluteRequestUrl => throw new NotImplementedException();

                /// <inheritdoc />
                public Uri ServiceBaseUrl => throw new NotImplementedException();

                /// <inheritdoc />
                public Uri ServiceRootUrl => throw new NotImplementedException();

                /// <inheritdoc />
                public Uri ControllerRelativeUrl => throw new NotImplementedException();

                /// <inheritdoc />
                public Uri ActionUrl => throw new NotImplementedException();

                /// <inheritdoc />
                public IUAParserOutput DetectedClient => throw new NotImplementedException();

                /// <inheritdoc />
                public IWebDavRequestHeaders RequestHeaders => throw new NotImplementedException();

                /// <inheritdoc />
                public IPrincipal User => throw new NotImplementedException();

                /// <inheritdoc />
                public IWebDavDispatcher Dispatcher => throw new NotImplementedException();
            }
        }
    }
}
