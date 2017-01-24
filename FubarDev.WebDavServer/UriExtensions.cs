using System;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer
{
    public static class UriExtensions
    {
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

        public static Uri Append(this Uri baseUri, string relative)
        {
            return baseUri.Append(relative, false);
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
