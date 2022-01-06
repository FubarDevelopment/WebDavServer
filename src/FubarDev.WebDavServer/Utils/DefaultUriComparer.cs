// <copyright file="DefaultUriComparer.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using FubarDev.WebDavServer.Properties;

namespace FubarDev.WebDavServer.Utils
{
    /// <summary>
    /// Default implementation of the <see cref="IUriComparer"/> interface.
    /// </summary>
    public class DefaultUriComparer : IUriComparer
    {
        private static readonly char[] _slash = { '/' };
        private readonly IWebDavContextAccessor? _contextAccessor;
        private readonly IWebDavContext? _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultUriComparer"/> class.
        /// </summary>
        /// <param name="contextAccessor">The accessor for the WebDAV context.</param>
        public DefaultUriComparer(IWebDavContextAccessor? contextAccessor = null)
        {
            _contextAccessor = contextAccessor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultUriComparer"/> class.
        /// </summary>
        /// <param name="context">The WebDAV context.</param>
        private DefaultUriComparer(IWebDavContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the WebDAV context.
        /// </summary>
        private IWebDavContext? Context => _contextAccessor?.WebDavContext ?? _context;

        /// <summary>
        /// Workaround to create an instance of this class.
        /// </summary>
        /// <param name="context">The WebDAV context.</param>
        /// <returns>The URI comparer.</returns>
        public static IUriComparer Create(IWebDavContext context)
        {
            return new DefaultUriComparer(context);
        }

        /// <inheritdoc />
        public UriComparisonResult Compare(Uri x, Uri y)
        {
            var publicControllerUrl = Context?.PublicControllerUrl;
            var infoX = new UriInfo(x, publicControllerUrl);
            var infoY = new UriInfo(y, publicControllerUrl);
            return Compare(infoX, infoY);
        }

        /// <inheritdoc />
        public bool IsSameHost(Uri x, Uri y)
        {
            var publicControllerUrl = Context?.PublicControllerUrl;
            var infoX = new UriInfo(x, publicControllerUrl);
            var infoY = new UriInfo(y, publicControllerUrl);
            return CompareHosts(infoX, infoY) == UriComparisonResult.Equal;
        }

        /// <inheritdoc />
        public bool IsThisServer(Uri uri)
        {
            return IsThisServer(uri, Context?.PublicControllerUrl);
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
            var partsX = pathX.Split(_slash, StringSplitOptions.RemoveEmptyEntries);
            var partsY = pathY.Split(_slash, StringSplitOptions.RemoveEmptyEntries);
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
                UriComparisonResult.PrecedingDifferentHost => UriComparisonResult.FollowingDifferentHost,
                UriComparisonResult.FollowingDifferentHost => UriComparisonResult.PrecedingDifferentHost,
                UriComparisonResult.PrecedingSibling => UriComparisonResult.FollowingSibling,
                UriComparisonResult.FollowingSibling => UriComparisonResult.PrecedingSibling,
                UriComparisonResult.Parent => UriComparisonResult.Child,
                UriComparisonResult.Child => UriComparisonResult.Parent,
                _ => result,
            };

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
        private static UriComparisonResult CompareWithAbsoluteUri(
            Uri absolute,
            Uri relative)
        {
            if (!absolute.IsAbsoluteUri)
            {
                throw new ArgumentException(Resources.UriMustBeAbsolute, nameof(absolute));
            }

            if (relative.IsAbsoluteUri)
            {
                throw new ArgumentException(Resources.UriMustNotBeAbsolute, nameof(relative));
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

        private static UriComparisonResult Compare(UriInfo infoX, UriInfo infoY)
        {
            if (!infoX.IsAbsolute && !infoY.IsAbsolute)
            {
                return CompareRelativePaths(infoX.PathWithSlash, infoY.PathWithSlash);
            }

            var hostCompareResult = CompareHosts(infoX, infoY);
            if (hostCompareResult != UriComparisonResult.Equal)
            {
                return hostCompareResult;
            }

            if (!infoX.IsAbsolute)
            {
                return InvertResult(CompareWithAbsoluteUri(infoY.Uri, infoX.Uri));
            }

            if (!infoY.IsAbsolute)
            {
                return CompareWithAbsoluteUri(infoX.Uri, infoY.Uri);
            }

            // Just assume that the root path is the root path of the WebDAV service
            return CompareRelativePaths(infoX.PathWithSlash, infoY.PathWithSlash);
        }

        private bool IsThisServer(Uri uri, Uri? publicControllerUrl)
        {
            if (Context is null)
            {
                // Always assume that the URL points to the same server, because we don't have a context
                return true;
            }

            var uriInfo = new UriInfo(uri, publicControllerUrl);
            if (!uriInfo.IsAbsolute || publicControllerUrl is null)
            {
                // URL is not absolute
                return true;
            }

            if (!IsSameHost(publicControllerUrl, uriInfo.Uri))
            {
                return false;
            }

            // This makes "http://localhost/_dav/" a base path of "/_dav/"
            var basePath = "/" + publicControllerUrl.GetComponents(UriComponents.Path, UriFormat.UriEscaped);
            if (!basePath.EndsWith("/"))
            {
                basePath += "/";
            }

            var uriPath = uriInfo.PathWithSlash;

            return uriPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase);
        }

        private class UriInfo
        {
            public UriInfo(Uri originalUri, Uri? publicBaseUri)
            {
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
                            uri = new Uri(publicBaseUri, "/" + uri.OriginalString.TrimStart('/'));
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
                            uri = new Uri("file://" + uri.OriginalString, UriKind.Absolute);
                        }
                    }
                }

                Uri = uri;
                IsAbsolute = uri.IsAbsoluteUri;

                Path = uri.IsAbsoluteUri
                    ? "/" + uri.GetComponents(UriComponents.Path, UriFormat.UriEscaped)
                    : new Uri("file:///" + uri.OriginalString).GetComponents(UriComponents.Path, UriFormat.UriEscaped);
                PathWithSlash = !Path.EndsWith("/") ? Path + "/" : Path;
            }

            public string Scheme { get; }

            public Uri Uri { get; }

            public bool IsAbsolute { get; }

            public string Path { get; }

            public string PathWithSlash { get; }
        }
    }
}
