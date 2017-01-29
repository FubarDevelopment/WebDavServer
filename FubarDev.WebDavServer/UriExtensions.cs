using System;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer
{
    public static class UriExtensions
    {
        public static Uri GetParent(this Uri url)
        {
            if (url.OriginalString.EndsWith("/"))
                return new Uri(url, "..");
            return new Uri(url, ".");
        }

        public static string GetName(this Uri url)
        {
            var s = url.OriginalString;
            var searchStartPos = s.EndsWith("/") ? s.Length - 2 : s.Length - 1;
            var slashIndex = s.LastIndexOf("/", searchStartPos, StringComparison.Ordinal);
            var length = searchStartPos - slashIndex + 1;
            var name = s.Substring(slashIndex + 1, length);
            return Uri.UnescapeDataString(name);
        }

        public static Uri Append(this Uri baseUri, IDocument entry)
        {
            return baseUri.Append(Uri.EscapeDataString(entry.Name), true);
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
            return baseUri.Append((ICollection) entry);
        }

        public static Uri AppendDirectory(this Uri baseUri, string relative)
        {
            return baseUri.Append(Uri.EscapeDataString(relative) + "/", true);
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

            return new Uri(basePath + (isEscaped ? relative : Uri.EscapeDataString(relative)), UriKind.RelativeOrAbsolute);
        }
    }
}
