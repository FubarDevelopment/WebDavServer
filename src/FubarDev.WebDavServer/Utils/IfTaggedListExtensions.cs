// <copyright file="IfTaggedListExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;

using FubarDev.WebDavServer.Models;

namespace FubarDev.WebDavServer.Utils;

/// <summary>
/// Extension methods for <see cref="IfTaggedList"/>.
/// </summary>
public static class IfTaggedListExtensions
{
    /// <summary>
    /// Try to get the path for the given tagged list.
    /// </summary>
    /// <param name="taggedList">The tagged list to get the path for.</param>
    /// <param name="context">The WebDAV context.</param>
    /// <param name="path">The found path.</param>
    /// <returns><see langword="true"/> if a path could be found.</returns>
    public static bool TryGetPath(
        this IfTaggedList taggedList,
        IWebDavContext context,
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

        path = context.PublicRootUrl.GetRelativeUrl(url).OriginalString;
        return true;
    }

    /// <summary>
    /// Try to get the HREF for the given tagged list.
    /// </summary>
    /// <param name="taggedList">The tagged list to get the HREF for.</param>
    /// <param name="context">The WebDAV context.</param>
    /// <param name="href">The found HREF.</param>
    /// <returns><see langword="true"/> if a path could be found.</returns>
    public static bool TryGetHref(
        this IfTaggedList taggedList,
        IWebDavContext context,
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
