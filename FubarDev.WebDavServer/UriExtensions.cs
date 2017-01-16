using System;

namespace FubarDev.WebDavServer
{
    internal static class UriExtensions
    {
        public static string ToEncoded(this Uri uri)
        {
            var result = uri.GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped).Replace("[", "%5B").Replace("]", "%5D");
            return result;
        }
    }
}
