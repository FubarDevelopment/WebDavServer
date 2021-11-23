// <copyright file="IUriComparer.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace FubarDev.WebDavServer.Utils
{
    /// <summary>
    /// Interface that provides a service for URL comparison.
    /// </summary>
    public interface IUriComparer : IComparer<Uri>
    {
        /// <summary>
        /// Compares two <see cref="Uri"/>s and returns extended information about the relationship.
        /// </summary>
        /// <param name="x">The URI to be used as base.</param>
        /// <param name="y">The URI to compare with.</param>
        /// <returns>The result of the comparison.</returns>
        new UriComparisonResult Compare(Uri x, Uri y);

        /// <summary>
        /// Returns a value indicating whether both URIs point to the same host.
        /// </summary>
        /// <param name="x">The URI to be used as base.</param>
        /// <param name="y">The URI to compare with.</param>
        /// <returns>The result of the comparison.</returns>
        bool IsSameHost(Uri x, Uri y);

        /// <summary>
        /// Gets a value indicating whether the URI points to a resource on this server.
        /// </summary>
        /// <param name="uri">The URI to the resource.</param>
        /// <returns><see langword="true"/>, when the URI points to a resource on this server.</returns>
        bool IsThisServer(Uri uri);
    }
}
