// <copyright file="UriExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// Extension methods for URLs.
    /// </summary>
    public static class UriExtensions
    {
        /// <summary>
        /// Gets the URL of the containing collection of a collection or document URL.
        /// </summary>
        /// <param name="url">The URL to get the parent URL for.</param>
        /// <returns>The URL of the containing collection.</returns>
        public static Uri GetParent(this Uri url)
        {
            if (url.IsAbsoluteUri)
            {
                if (url.OriginalString.EndsWith("/"))
                {
                    return new Uri(url, "..");
                }

                return new Uri(url, ".");
            }

            var temp = url.OriginalString;
            if (temp.EndsWith("/"))
            {
                temp = temp.Substring(0, temp.Length - 1);
            }

            var p = temp.LastIndexOf("/", StringComparison.Ordinal);
            if (p == -1)
            {
                return new Uri(string.Empty, UriKind.Relative);
            }

            return new Uri(temp.Substring(0, p + 1), UriKind.Relative);
        }

        /// <summary>
        /// Gets the collection URL.
        /// </summary>
        /// <remarks>
        /// This is different from <see cref="GetParent"/>, because this function returns
        /// the same URL if the <paramref name="url"/> is already an URL to a collection.
        /// </remarks>
        /// <param name="url">The URL to get the collection for.</param>
        /// <returns>The collection URL.</returns>
        /// <remarks>
        /// When the <paramref name="url"/> is already a collection URL, then this URL will be
        /// returned.
        /// </remarks>
        public static Uri GetCollectionUri(this Uri url)
        {
            if (url.IsAbsoluteUri)
            {
                return new Uri(url, ".");
            }

            var temp = url.OriginalString;
            if (temp.EndsWith("/"))
            {
                return url;
            }

            var p = temp.LastIndexOf("/", StringComparison.Ordinal);
            if (p == -1)
            {
                return new Uri(string.Empty, UriKind.Relative);
            }

            return new Uri(temp.Substring(0, p + 1), UriKind.Relative);
        }

        /// <summary>
        /// Gets the name of the collection or document URL.
        /// </summary>
        /// <param name="url">The collection or document URL.</param>
        /// <returns>The name of the collection or document URL.</returns>
        public static string GetName(this Uri url)
        {
            var s = url.OriginalString;
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            var searchStartPos = s.EndsWith("/") ? s.Length - 2 : s.Length - 1;
            var slashIndex = s.LastIndexOf("/", searchStartPos, StringComparison.Ordinal);
            var length = searchStartPos - slashIndex;
            var name = s.Substring(slashIndex + 1, length);
            return Uri.UnescapeDataString(name);
        }

        /// <summary>
        /// Appends the name of a document to the <paramref name="baseUri"/>.
        /// </summary>
        /// <param name="baseUri">The base URL to append the <paramref name="entry"/> name to.</param>
        /// <param name="entry">The <see cref="IDocument"/> whose name to append to the <paramref name="baseUri"/>.</param>
        /// <returns>The <paramref name="baseUri"/> with the <paramref name="entry"/> name appended.</returns>
        public static Uri Append(this Uri baseUri, IDocument entry)
        {
            return baseUri.Append(entry.Name.UriEscape(), true);
        }

        /// <summary>
        /// Appends the name of a collection to the <paramref name="baseUri"/>.
        /// </summary>
        /// <param name="baseUri">The base URL to append the <paramref name="entry"/> name to.</param>
        /// <param name="entry">The <see cref="ICollection"/> whose name to append to the <paramref name="baseUri"/>.</param>
        /// <returns>The <paramref name="baseUri"/> with the <paramref name="entry"/> name appended.</returns>
        public static Uri Append(this Uri baseUri, ICollection entry)
        {
            return baseUri.AppendDirectory(entry.Name);
        }

        /// <summary>
        /// Append the name of an <paramref name="entry"/> to the <paramref name="baseUri"/>.
        /// </summary>
        /// <param name="baseUri">The base URL to append the <paramref name="entry"/> name to.</param>
        /// <param name="entry">The <see cref="IEntry"/> whose name to append to the <paramref name="baseUri"/>.</param>
        /// <returns>The <paramref name="baseUri"/> with the <paramref name="entry"/> name appended.</returns>
        public static Uri Append(this Uri baseUri, IEntry entry)
        {
            var doc = entry as IDocument;
            if (doc != null)
            {
                return baseUri.Append(doc);
            }

            return baseUri.Append((ICollection)entry);
        }

        /// <summary>
        /// Appends a collection name to the <paramref name="baseUri"/>.
        /// </summary>
        /// <param name="baseUri">The base URL to append the name to.</param>
        /// <param name="collectionName">The collection name to append.</param>
        /// <returns>The <paramref name="baseUri"/> with the appended name.</returns>
        public static Uri AppendDirectory(this Uri baseUri, string collectionName)
        {
            return baseUri.Append(collectionName.UriEscape() + "/", true);
        }

        /// <summary>
        /// Append a relative URI to the <paramref name="baseUri"/>.
        /// </summary>
        /// <param name="baseUri">The base URL to append the <paramref name="relativeUri"/> to.</param>
        /// <param name="relativeUri">The relative URL to append to the <paramref name="baseUri"/>.</param>
        /// <returns>The <paramref name="baseUri"/> with the appended <paramref name="relativeUri"/>.</returns>
        public static Uri Append(this Uri baseUri, Uri relativeUri)
        {
            return baseUri.Append(relativeUri.OriginalString, true);
        }

        /// <summary>
        /// Append a relative URL to the <paramref name="baseUri"/>.
        /// </summary>
        /// <param name="baseUri">The base URL to append the <paramref name="relative"/> URI to.</param>
        /// <param name="relative">The relative URL to append to the <paramref name="baseUri"/>.</param>
        /// <param name="isEscaped">Indicates whether the <paramref name="relative"/> URL is already escaped.</param>
        /// <returns>The <paramref name="baseUri"/> with the appended <paramref name="relative"/> URL.</returns>
        /// <remarks>
        /// When the <paramref name="relative"/> URL isn't escaped, then it MUST NOT contain a path separator.
        /// </remarks>
        public static Uri Append(this Uri baseUri, string relative, bool isEscaped)
        {
            var basePath = baseUri.OriginalString;
            if (!basePath.EndsWith("/"))
            {
                var p = basePath.LastIndexOf("/", StringComparison.Ordinal);
                basePath = p == -1 ? string.Empty : basePath.Substring(0, p + 1);
            }

            return new Uri(basePath + (isEscaped ? relative : relative.UriEscape()), UriKind.RelativeOrAbsolute);
        }

        /// <summary>
        /// Escapes the string in a WebDAV compatible way.
        /// </summary>
        /// <param name="s">The string to escape.</param>
        /// <returns>The escaped string.</returns>
        public static string UriEscape(this string s)
        {
            return Uri.EscapeDataString(s);
        }

        /// <summary>
        /// Encodes a HREF URI.
        /// </summary>
        /// <param name="uri">The URI to encode.</param>
        /// <param name="encode">A value indicating whether the URI should be encoded.</param>
        /// <returns>The encoded URI.</returns>
        internal static string EncodeHref(this Uri uri, bool encode = true)
        {
            string result;
            if (!encode)
            {
                result = uri.OriginalString;
            }
            else
            {
                if (!uri.IsAbsoluteUri)
                {
                    uri = new Uri(new Uri("http://localhost/"), uri);
                }

                result = uri.GetComponents(UriComponents.Path, UriFormat.UriEscaped);

                // Litmus test specific... WUT?
                result = result.Replace("_", "%5f");
            }

            if (!result.StartsWith("/"))
            {
                result = "/" + result;
            }

            return result;
        }
    }
}
