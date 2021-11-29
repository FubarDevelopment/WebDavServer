// <copyright file="UriComparerTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;

using FubarDev.WebDavServer.Utils;
using FubarDev.WebDavServer.Utils.UAParser;

using Xunit;

namespace FubarDev.WebDavServer.Tests
{
    public class UriComparerTests
    {
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
        [InlineData("/a", UriComparisonResult.PrecedingDifferentHost)]
        public void TestCompare(string url, UriComparisonResult expected)
        {
            var baseUrl = new Uri("http://localhost/_dav");
            var comparer = new DefaultUriComparer(new TestWebDavContextAccessor(baseUrl));
            var result = comparer.Compare(new Uri(url, UriKind.RelativeOrAbsolute), baseUrl);
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
                public TestWebDavContext(Uri publicBaseUrl)
                {
                    PublicBaseUrl = publicBaseUrl;
                }

                /// <inheritdoc />
                public IServiceProvider RequestServices => throw new NotImplementedException();

                /// <inheritdoc />
                public string RequestProtocol => throw new NotImplementedException();

                /// <inheritdoc />
                public string RequestMethod => throw new NotImplementedException();

                /// <inheritdoc />
                public Uri PublicRelativeRequestUrl => throw new NotImplementedException();

                /// <inheritdoc />
                public Uri PublicAbsoluteRequestUrl => throw new NotImplementedException();

                /// <inheritdoc />
                public Uri PublicControllerUrl => throw new NotImplementedException();

                /// <inheritdoc />
                public Uri PublicBaseUrl { get; }

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

        /// <summary>
        /// Default implementation of the <see cref="IUriComparer"/> interface.
        /// </summary>
        private class DefaultUriComparer : IUriComparer
        {
            private readonly IWebDavContextAccessor? _contextAccessor;

            /// <summary>
            /// Initializes a new instance of the <see cref="DefaultUriComparer"/> class.
            /// </summary>
            /// <param name="contextAccessor">The accessor for the WebDAV context.</param>
            public DefaultUriComparer(IWebDavContextAccessor? contextAccessor = null)
            {
                _contextAccessor = contextAccessor;
            }

            /// <inheritdoc />
            public UriComparisonResult Compare(Uri x, Uri y)
            {
                var context = _contextAccessor?.WebDavContext;
                var publicBaseUrl = context?.PublicBaseUrl;
                var infoX = new UriInfo(x, publicBaseUrl);
                var infoY = new UriInfo(y, publicBaseUrl);
                if (!infoX.IsAbsolute && !infoY.IsAbsolute)
                {
                    return CompareRelativePaths(infoX.PathWithSlash, infoY.PathWithSlash);
                }

                if (TryGetAbsolute(x, out var absoluteX))
                {
                    x = absoluteX;
                }

                if (TryGetAbsolute(y, out var absoluteY))
                {
                    y = absoluteY;
                }

                if (!IsSameHost(x, y))
                {
                    return UriComparisonResult.PrecedingDifferentHost;
                }

                var uriX = MakeAbsoluteUri(x, context);
                var uriY = MakeAbsoluteUri(y, context);

                if (uriX is null)
                {
                    if (uriY is null)
                    {
                        // Both are relative URLs, so let's make them absolute
                        return CompareAbsoluteUris(
                            new Uri("/" + x.OriginalString),
                            new Uri("/" + y.OriginalString));
                    }

                    return InvertResult(CompareWithAbsoluteUri(uriY, x));
                }

                if (uriY is null)
                {
                    return CompareWithAbsoluteUri(uriX, y);
                }

                return CompareAbsoluteUris(uriX, uriY);
            }

            /// <inheritdoc />
            public bool IsSameHost(Uri x, Uri y)
            {
                var publicBaseUrl = _contextAccessor?.WebDavContext.PublicBaseUrl;
                var infoX = new UriInfo(x, publicBaseUrl);
                var infoY = new UriInfo(y, publicBaseUrl);
                return CompareHosts(infoX, infoY) == UriComparisonResult.Equal;
            }

            /// <inheritdoc />
            public bool IsThisServer(Uri uri)
            {
                return IsThisServer(uri, _contextAccessor?.WebDavContext.PublicBaseUrl);
            }

            /// <inheritdoc />
            int IComparer<Uri>.Compare(Uri? x, Uri? y)
            {
                if (x is null)
                {
                    if (y is null)
                    {
                        return 0;
                    }

                    return -10;
                }

                if (y is null)
                {
                    return 10;
                }

                return (int)Compare(x, y);
            }

            private static UriComparisonResult CompareRelativePaths(string pathX, string pathY)
            {
                var partsX = pathX.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var partsY = pathY.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var minLength = Math.Min(partsX.Length, partsY.Length);
                for (var i = 0; i != minLength; ++i)
                {
                    var partX = partsX[i];
                    var partY = partsY[i];
                    var partCompare = string.Compare(partX, partY, StringComparison.OrdinalIgnoreCase);
                    switch (partCompare)
                    {
                        case < 0:
                            return UriComparisonResult.FollowingSibling;
                        case > 0:
                            return UriComparisonResult.PrecedingSibling;
                    }
                }

                return partsX.Length == partsY.Length
                    ? UriComparisonResult.Equal
                    : partsX.Length > partsY.Length
                        ? UriComparisonResult.Child
                        : UriComparisonResult.Parent;
            }

            private static UriComparisonResult CompareHosts(UriInfo infoX, UriInfo infoY)
            {
                if (!infoX.IsAbsolute
                    || !infoY.IsAbsolute
                    || string.IsNullOrEmpty(infoX.Uri.Host)
                    || string.IsNullOrEmpty(infoY.Uri.Host))
                {
                    // It's the same host if one of them are relative URIs
                    // or the host is empty
                    return UriComparisonResult.Equal;
                }

                var uriComponents = string.IsNullOrEmpty(infoX.Scheme) || string.IsNullOrEmpty(infoX.Scheme)
                    ? UriComponents.Host | UriComponents.Port
                    : UriComponents.Scheme | UriComponents.HostAndPort;
                var valueX = infoX.Uri.GetComponents(uriComponents, UriFormat.UriEscaped);
                var valueY = infoY.Uri.GetComponents(uriComponents, UriFormat.UriEscaped);

                return string.Compare(valueX, valueY, StringComparison.OrdinalIgnoreCase) switch
                {
                    < 0 => UriComparisonResult.PrecedingDifferentHost,
                    0 => UriComparisonResult.Equal,
                    > 0 => UriComparisonResult.FollowingDifferentHost,
                };
            }

            private static UriComparisonResult InvertResult(UriComparisonResult result)
                => result switch
                {
                    UriComparisonResult.PrecedingSibling => UriComparisonResult.FollowingSibling,
                    UriComparisonResult.FollowingSibling => UriComparisonResult.PrecedingSibling,
                    UriComparisonResult.Parent => UriComparisonResult.Child,
                    UriComparisonResult.Child => UriComparisonResult.Parent,
                    _ => result,
                };

            private static string FixUriPath(string path)
            {
                path = path.TrimEnd('/');
                if (string.IsNullOrEmpty(path))
                {
                    return "/";
                }

                if (!path.StartsWith("/"))
                {
                    return "/" + path;
                }

                return path;
            }

            private static bool TryGetAbsolute(Uri uri, [NotNullWhen(true)] out Uri? absoluteUri)
            {
                if (uri.IsAbsoluteUri)
                {
                    absoluteUri = uri;
                    return true;
                }

                var originalString = uri.OriginalString;
                if (originalString.StartsWith("/"))
                {
                    absoluteUri = new Uri(originalString);
                    return true;
                }

                absoluteUri = null;
                return false;
            }

            /// <summary>
            /// Compares a relative URL with an absolute URL.
            /// </summary>
            /// <param name="absolute">The absolute URL.</param>
            /// <param name="relative">The relative URL.</param>
            /// <returns>The result of the comparison.</returns>
            /// <exception cref="ArgumentException">The absolute URL is not absolute or the relative isn't
            /// relative.</exception>
            /// <remarks>
            /// This is all just guesswork!
            /// </remarks>
            private UriComparisonResult CompareWithAbsoluteUri(
                Uri absolute,
                Uri relative)
            {
                if (!absolute.IsAbsoluteUri)
                {
                    throw new ArgumentException("The URI must be absolute", nameof(absolute));
                }

                if (!relative.IsAbsoluteUri)
                {
                    throw new ArgumentException("The URI must not be absolute", nameof(relative));
                }

                var relativePathParts = relative.OriginalString
                    .Split('/')
                    .Select(Uri.UnescapeDataString)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();
                if (relativePathParts.Count == 0)
                {
                    return UriComparisonResult.Equal;
                }

                var absolutePathParts = absolute.GetComponents(UriComponents.Path, UriFormat.UriEscaped)
                    .Split('/')
                    .Select(Uri.UnescapeDataString)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();
                if (absolutePathParts.Count == 0)
                {
                    return UriComparisonResult.Parent;
                }

                // Find the parts that match the absolute path parts
                var startIndex = 0;
                var foundCount = 0;
                for (var i = 0; i != relativePathParts.Count; ++i)
                {
                    var relativePart = relativePathParts[i];
                    var foundIndex = absolutePathParts.FindIndex(
                        startIndex,
                        x => string.Equals(x, relativePart, StringComparison.OrdinalIgnoreCase));
                    if (foundIndex == -1)
                    {
                        break;
                    }

                    if (foundCount != 0 && foundIndex != startIndex)
                    {
                        break;
                    }

                    startIndex = foundIndex + 1;
                    foundCount += 1;
                }

                if (foundCount == 0)
                {
                    // The absolute path seems to be the parent of the relative path
                    return UriComparisonResult.Parent;
                }

                if (foundCount == relativePathParts.Count)
                {
                    // Relative path was found in absolute path
                    if (startIndex == absolutePathParts.Count)
                    {
                        // The absolute path ends with the relative path
                        return UriComparisonResult.Equal;
                    }

                    // The relative path seems to be the parent of the absolute path
                    return UriComparisonResult.Child;
                }

                if (startIndex == absolutePathParts.Count)
                {
                    // The relative path seems to be the child of the absolute path
                    return UriComparisonResult.Child;
                }

                var nextAbsolutePart = absolutePathParts[startIndex];
                var nextRelativePart = relativePathParts[foundCount];
                var result = string.Compare(nextAbsolutePart, nextRelativePart, StringComparison.OrdinalIgnoreCase);
                Debug.Assert(result != 0, "The next parts should not be equal");

                return result < 0
                    ? UriComparisonResult.PrecedingSibling
                    : UriComparisonResult.FollowingSibling;
            }

            private UriComparisonResult CompareAbsoluteUris(
                Uri x,
                Uri y)
            {
                if (!x.IsAbsoluteUri)
                {
                    throw new ArgumentException("The URI must be absolute", nameof(x));
                }

                if (!y.IsAbsoluteUri)
                {
                    throw new ArgumentException("The URI must be absolute", nameof(y));
                }

                var pathX = FixUriPath(x.GetComponents(UriComponents.Path, UriFormat.UriEscaped));
                var pathY = FixUriPath(y.GetComponents(UriComponents.Path, UriFormat.UriEscaped));
                if (string.Equals(pathX, pathY, StringComparison.OrdinalIgnoreCase))
                {
                    return UriComparisonResult.Equal;
                }

                var uriX = new Uri(pathX);
                var uriY = new Uri(pathY);
                return uriX.IsBaseOf(uriY)
                    ? UriComparisonResult.Child
                    : UriComparisonResult.Parent;
            }

            private bool IsThisServer(Uri uri, Uri? publicBaseUrl)
            {
                if (_contextAccessor is null)
                {
                    // Always assume that the URL points to the same server, because we don't have a context
                    return true;
                }

                var uriInfo = new UriInfo(uri, publicBaseUrl);
                if (!uriInfo.IsAbsolute)
                {
                    // URL is not absolute
                    return true;
                }

                if (!IsSameHost(publicBaseUrl, uriInfo.Uri))
                {
                    return false;
                }

                // This makes "http://localhost/_dav/" a base path of "/_dav/"
                var basePath = "/" + publicBaseUrl.GetComponents(UriComponents.Path, UriFormat.UriEscaped);
                if (!basePath.EndsWith("/"))
                {
                    basePath += "/";
                }

                var uriPath = uriInfo.PathWithSlash;

                return uriPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase);
            }

            private Uri? MakeAbsoluteUri(Uri uri, IWebDavContext? context = null)
            {
                if (uri.IsAbsoluteUri)
                {
                    return uri;
                }

                context ??= _contextAccessor?.WebDavContext;
                if (context == null)
                {
                    // It is "absolute" in the sense that it is relative to the server root
                    if (uri.OriginalString.StartsWith("/"))
                    {
                        return new Uri(uri.OriginalString);
                    }

                    // Not possible without a context
                    return null;
                }

                return new Uri(context.PublicBaseUrl, uri);
            }
        }

        private class UriInfo
        {
            public UriInfo(Uri originalUri, Uri? publicBaseUri)
            {
                OriginalUri = originalUri;
                Scheme = publicBaseUri?.Scheme ?? "http";

                var uri = originalUri;
                if (!uri.IsAbsoluteUri)
                {
                    if (publicBaseUri != null)
                    {
                        var basePath = publicBaseUri.OriginalString;
                        if (!basePath.EndsWith("/"))
                        {
                            // Always ensure that the path ends in a slash
                            publicBaseUri = new Uri(basePath + "/");
                        }

                        if (uri.OriginalString.StartsWith("///"))
                        {
                            uri = new Uri(publicBaseUri, uri.OriginalString.TrimStart('/'));
                        }
                        else if (uri.OriginalString.StartsWith("//"))
                        {
                            uri = new Uri(Scheme + "://" + uri.OriginalString.TrimStart('/'));
                        }
                        else
                        {
                            uri = new Uri(publicBaseUri, uri);
                        }
                    }
                    else
                    {
                        if (uri.OriginalString.StartsWith("///"))
                        {
                            uri = new Uri("/" + uri.OriginalString.TrimStart('/'));
                        }
                        else if (uri.OriginalString.StartsWith("//"))
                        {
                            uri = new Uri(Scheme + "://" + uri.OriginalString.TrimStart('/'));
                        }
                        else if (uri.OriginalString.StartsWith("/"))
                        {
                            uri = new Uri(uri.OriginalString);
                        }
                    }
                }

                Uri = uri;
                IsAbsolute = uri.IsAbsoluteUri;

                Path = uri.IsAbsoluteUri
                    ? "/" + uri.GetComponents(UriComponents.Path, UriFormat.UriEscaped)
                    : new Uri("/" + uri.OriginalString).GetComponents(UriComponents.Path, UriFormat.UriEscaped);
                PathWithSlash = !Path.EndsWith("/") ? Path + "/" : Path;
            }

            public string Scheme { get; }

            public Uri OriginalUri { get; }

            public Uri Uri { get; }

            public bool IsAbsolute { get; }

            public string Path { get; }

            public string PathWithSlash { get; }
        }
    }
}
