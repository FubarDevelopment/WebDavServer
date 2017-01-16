using System;

namespace FubarDev.WebDavServer
{
    public static class UriExtensions
    {
        public static Uri Append(this Uri baseUri, string relative)
        {
            var basePath = baseUri.OriginalString;
            if (!basePath.EndsWith("/"))
            {
                var p = basePath.LastIndexOf("/", StringComparison.Ordinal);
                basePath = p == -1 ? string.Empty : basePath.Substring(0, p + 1);
            }

            return new Uri(basePath + relative, UriKind.RelativeOrAbsolute);
        }

        public static Uri Append(this Uri baseUri, Uri relativeUri)
        {
            return baseUri.Append(relativeUri.OriginalString);
        }
    }
}
