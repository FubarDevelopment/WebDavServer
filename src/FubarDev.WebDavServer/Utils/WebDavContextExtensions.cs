// <copyright file="WebDavContextExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using FubarDev.WebDavServer.Models;

namespace FubarDev.WebDavServer.Utils;

/// <summary>
/// Extension methods around the <see cref="IWebDavContext"/>.
/// </summary>
public static class WebDavContextExtensions
{
    /// <summary>
    /// Gets the HREF or resource tag for a given match.
    /// </summary>
    /// <param name="context">The WebDAV context.</param>
    /// <param name="match">The <c>If</c> header match.</param>
    /// <returns>The HREF or resource tag.</returns>
    public static Uri GetHrefOrResourceTag(
        this IWebDavContext context,
        IfHeaderMatch match)
    {
        if (match.IsTaggedList)
        {
            if (context.TryGetHrefFor(match.TaggedList, out var href))
            {
                return href;
            }

            return match.TaggedList.ResourceTag;
        }

        return context.HrefUrl;
    }

    /// <summary>
    /// Get all HREFs or resource tags from the given <paramref name="header"/>.
    /// </summary>
    /// <param name="context">The WebDAV context.</param>
    /// <param name="header">The <c>If</c> header.</param>
    /// <returns>The found HREFs or resource tags.</returns>
    public static IEnumerable<Uri> GetHrefOrResourceTag(
        this IWebDavContext context,
        IfHeader header)
    {
        if (header.IsTaggedList)
        {
            foreach (var taggedList in header.TaggedLists)
            {
                if (context.TryGetHrefFor(taggedList, out var href))
                {
                    yield return href;
                }
                else
                {
                    yield return taggedList.ResourceTag;
                }
            }
        }
        else
        {
            yield return context.HrefUrl;
        }
    }

    /// <summary>
    /// Gets a HREF for a given path.
    /// </summary>
    /// <param name="context">The WebDAV context.</param>
    /// <param name="path">The path to get the HREF for.</param>
    /// <returns>The HREF.</returns>
    public static Uri GetHrefFor(this IWebDavContext context, string path)
    {
        var absoluteUrl = context.PublicControllerUrl.Append(path, true);
        var relativeUrl = context.PublicRootUrl.GetRelativeUrl(absoluteUrl);
        return new Uri(
            "/" + relativeUrl.OriginalString.TrimStart('/'),
            UriKind.Relative);
    }

    /// <summary>
    /// Try to get the path for the given tagged list.
    /// </summary>
    /// <param name="context">The WebDAV context.</param>
    /// <param name="taggedList">The tagged list to get the path for.</param>
    /// <param name="path">The found path.</param>
    /// <returns><see langword="true"/> if a path could be found.</returns>
    public static bool TryGetPathFor(
        this IWebDavContext context,
        IfTaggedList taggedList,
        [NotNullWhen(true)] out string? path)
    {
        var url = new Uri(context.PublicRootUrl, taggedList.ResourceTag.OriginalString);
        if (!context.PublicControllerUrl.IsBaseOf(url))
        {
            if (context.PublicControllerUrl == url)
            {
                path = string.Empty;
                return true;
            }

            path = null;
            return false;
        }

        // Unescape!
        path = context.PublicRootUrl.GetRelativeUrl(url).ToString();
        return true;
    }

    /// <summary>
    /// Try to get the HREF for the given tagged list.
    /// </summary>
    /// <param name="context">The WebDAV context.</param>
    /// <param name="taggedList">The tagged list to get the HREF for.</param>
    /// <param name="href">The found HREF.</param>
    /// <returns><see langword="true"/> if a path could be found.</returns>
    public static bool TryGetHrefFor(
        this IWebDavContext context,
        IfTaggedList taggedList,
        [NotNullWhen(true)] out Uri? href)
    {
        var url = new Uri(context.PublicRootUrl, taggedList.ResourceTag.OriginalString);
        if (!context.PublicRootUrl.IsBaseOf(url))
        {
            if (context.PublicRootUrl == url)
            {
                href = new Uri("/", UriKind.Relative);
                return true;
            }

            href = null;
            return false;
        }

        href = new Uri(
            "/" + context.PublicRootUrl.GetRelativeUrl(url).OriginalString.TrimStart('/'),
            UriKind.Relative);
        return true;
    }
}
