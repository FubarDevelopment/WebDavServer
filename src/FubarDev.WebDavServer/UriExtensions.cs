// <copyright file="UriExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer
{
    public static class UriExtensions
    {
        public static Uri GetParent(this Uri url)
        {
            if (url.IsAbsoluteUri)
            {
                if (url.OriginalString.EndsWith("/"))
                    return new Uri(url, "..");
                return new Uri(url, ".");
            }

            var temp = url.OriginalString;
            if (temp.EndsWith("/"))
                temp = temp.Substring(0, temp.Length - 1);
            var p = temp.LastIndexOf("/", StringComparison.Ordinal);
            if (p == -1)
                return new Uri(string.Empty, UriKind.Relative);
            return new Uri(temp.Substring(0, p + 1), UriKind.Relative);
        }

        public static Uri GetCollectionUri(this Uri url)
        {
            if (url.IsAbsoluteUri)
                return new Uri(url, ".");

            var temp = url.OriginalString;
            if (temp.EndsWith("/"))
                return url;
            var p = temp.LastIndexOf("/", StringComparison.Ordinal);
            if (p == -1)
                return new Uri(string.Empty, UriKind.Relative);
            return new Uri(temp.Substring(0, p + 1), UriKind.Relative);
        }

        public static string GetName(this Uri url)
        {
            var s = url.OriginalString;
            var searchStartPos = s.EndsWith("/") ? s.Length - 2 : s.Length - 1;
            var slashIndex = s.LastIndexOf("/", searchStartPos, StringComparison.Ordinal);
            var length = searchStartPos - slashIndex;
            var name = s.Substring(slashIndex + 1, length);
            return Uri.UnescapeDataString(name);
        }

        public static Uri Append(this Uri baseUri, IDocument entry)
        {
            return baseUri.Append(entry.Name.UriEscape(), true);
        }

        public static Uri Append(this Uri baseUri, ICollection entry)
        {
            return baseUri.AppendDirectory(entry.Name);
        }

        public static Uri Append(this Uri baseUri, IEntry entry)
        {
            var doc = entry as IDocument;
            if (doc != null)
                return baseUri.Append(doc);
            return baseUri.Append((ICollection)entry);
        }

        public static Uri AppendDirectory(this Uri baseUri, string relative)
        {
            return baseUri.Append(relative.UriEscape() + "/", true);
        }

        public static Uri Append(this Uri baseUri, Uri relativeUri)
        {
            return baseUri.Append(relativeUri.OriginalString, true);
        }

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

        public static string UriEscape(this string s)
        {
            return Uri.EscapeDataString(s);
        }
    }
}
