// <copyright file="DefaultUriComparer.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace FubarDev.WebDavServer.Utils
{
    /// <summary>
    /// Default implementation of the <see cref="IUriComparer"/> interface.
    /// </summary>
    public class DefaultUriComparer : IUriComparer
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

            var context = _contextAccessor?.WebDavContext;
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
            if (!TryGetAbsolute(x, out var absoluteX)
                || !TryGetAbsolute(y, out var absoluteY)
                || string.IsNullOrEmpty(absoluteX.Host)
                || string.IsNullOrEmpty(absoluteY.Host))
            {
                // It's the same host if one of them are relative URIs
                // or the host is empty
                return true;
            }

            x = absoluteX;
            y = absoluteY;

            var uriComponents = string.IsNullOrEmpty(x.Scheme) || string.IsNullOrEmpty(y.Scheme)
                ? UriComponents.Host | UriComponents.Port
                : UriComponents.Scheme | UriComponents.HostAndPort;
            var valueX = x.GetComponents(uriComponents, UriFormat.UriEscaped);
            var valueY = y.GetComponents(uriComponents, UriFormat.UriEscaped);

            return string.Equals(valueX, valueY, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public bool IsThisServer(Uri uri)
        {
            return IsThisServer(uri, null);
        }

        /// <inheritdoc />
        int IComparer<Uri>.Compare(Uri x, Uri y)
        {
            return (int)Compare(x, y);
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

        private bool IsThisServer(Uri uri, IWebDavContext? context)
        {
            if (_contextAccessor is null)
            {
                // Always assume that the URL points to the same server, because we don't have a context
                return true;
            }

            if (!TryGetAbsolute(uri, out var absoluteUri))
            {
                // Relative URIs are always the same server
                return true;
            }

            uri = absoluteUri;

            context ??= _contextAccessor.WebDavContext;
            var publicBaseUrl = context.PublicBaseUrl;

            if (string.IsNullOrEmpty(uri.Host) && uri.Scheme == "file" && uri.AbsolutePath.StartsWith("//"))
            {
                // This is a relative URI with a host but no scheme
                uri = new Uri(publicBaseUrl.Scheme + ":" + uri.AbsolutePath);
            }

            if (!string.IsNullOrEmpty(uri.Host))
            {
                if (!IsSameHost(publicBaseUrl, uri))
                {
                    return false;
                }
            }

            if (string.IsNullOrEmpty(publicBaseUrl.AbsolutePath) || publicBaseUrl.AbsolutePath == "/")
            {
                return true;
            }

            if (string.IsNullOrEmpty(uri.AbsolutePath) || uri.AbsolutePath == "/")
            {
                return false;
            }

            var basePathText = "/" + publicBaseUrl.GetComponents(UriComponents.Path, UriFormat.UriEscaped).TrimEnd('/');
            if (!basePathText.EndsWith("/"))
            {
                basePathText += "/";
            }

            var uriPathText = "/" + uri.GetComponents(UriComponents.Path, UriFormat.UriEscaped).TrimEnd('/');
            if (!uriPathText.EndsWith("/"))
            {
                uriPathText += "/";
            }

            var basePath = new Uri(basePathText);
            var uriPath = new Uri(uriPathText);

            // This seems to be case-sensitive. Is that really what we want/need?
            return basePath.IsBaseOf(uriPath);
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
}
