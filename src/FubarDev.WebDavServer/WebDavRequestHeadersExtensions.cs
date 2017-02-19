// <copyright file="WebDavRequestHeadersExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using FubarDev.WebDavServer.Model.Headers;

namespace FubarDev.WebDavServer
{
    public static class WebDavRequestHeadersExtensions
    {
        public static IEnumerable<IIfMatcher> GetIfHeaderMatchers(this IWebDavRequestHeaders headers)
        {
            if (headers.IfMatch != null)
                yield return headers.IfMatch;
            if (headers.IfNoneMatch != null)
                yield return headers.IfNoneMatch;
            if (headers.IfModifiedSince != null)
                yield return headers.IfModifiedSince;
            if (headers.IfUnmodifiedSince != null)
                yield return headers.IfUnmodifiedSince;
            if (headers.If != null)
                yield return headers.If;
        }
    }
}
