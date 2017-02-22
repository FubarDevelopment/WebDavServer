// <copyright file="RequestHeaderExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Utils
{
    internal static class RequestHeaderExtensions
    {
        [NotNull]
        public static async Task ValidateAsync([NotNull] this IWebDavRequestHeaders headers, [NotNull] IEntry entry, CancellationToken cancellationToken)
        {
            if (headers.IfMatch != null || headers.IfNoneMatch != null)
            {
                // Validate against ETag
                var etag = await entry.GetEntityTagAsync(cancellationToken).ConfigureAwait(false);
                if (headers.IfMatch != null && !headers.IfMatch.IsMatch(etag))
                    throw new WebDavException(WebDavStatusCode.PreconditionFailed);
                if (headers.IfNoneMatch != null && !headers.IfNoneMatch.IsMatch(etag))
                    throw new WebDavException(WebDavStatusCode.NotModified);
            }

            if (headers.IfModifiedSince != null || headers.IfUnmodifiedSince != null)
            {
                // Validate against last modification time
                var lastWriteTimeUtc = entry.LastWriteTimeUtc;
                if (headers.IfUnmodifiedSince != null && !headers.IfUnmodifiedSince.IsMatch(lastWriteTimeUtc))
                    throw new WebDavException(WebDavStatusCode.PreconditionFailed);
                if (headers.IfModifiedSince != null && !headers.IfModifiedSince.IsMatch(lastWriteTimeUtc))
                    throw new WebDavException(WebDavStatusCode.NotModified);
            }
        }
    }
}
