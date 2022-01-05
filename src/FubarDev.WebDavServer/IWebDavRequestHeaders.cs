// <copyright file="IWebDavRequestHeaders.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using FubarDev.WebDavServer.Model.Headers;

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// Interface for the access to the request headers
    /// </summary>
    public interface IWebDavRequestHeaders
    {
        /// <summary>
        /// Gets the <c>Content-Length</c> header.
        /// </summary>
        long? ContentLength { get; }

        /// <summary>
        /// Gets the <c>Depth</c> header.
        /// </summary>
        DepthHeader? Depth { get; }

        /// <summary>
        /// Gets the value of the <c>Overwrite</c> (<see cref="OverwriteHeader"/>) header.
        /// </summary>
        bool? Overwrite { get; }

        /// <summary>
        /// Gets the <c>If</c> header.
        /// </summary>
        IReadOnlyList<IfHeader>? If { get; }

        /// <summary>
        /// Gets the <c>If-Match</c> header.
        /// </summary>
        IfMatchHeader? IfMatch { get; }

        /// <summary>
        /// Gets the <c>If-None-Match</c> header.
        /// </summary>
        IfNoneMatchHeader? IfNoneMatch { get; }

        /// <summary>
        /// Gets the <c>If-Modified-Since</c> header.
        /// </summary>
        IfModifiedSinceHeader? IfModifiedSince { get; }

        /// <summary>
        /// Gets the <c>If-Unmodified-Since</c> header.
        /// </summary>
        IfUnmodifiedSinceHeader? IfUnmodifiedSince { get; }

        /// <summary>
        /// Gets the <c>Range</c> header.
        /// </summary>
        RangeHeader? Range { get; }

        /// <summary>
        /// Gets the <c>Timeout</c> header.
        /// </summary>
        TimeoutHeader? Timeout { get; }

        /// <summary>
        /// Gets all headers.
        /// </summary>
        IReadOnlyDictionary<string, IReadOnlyList<string>> Headers { get; }

        /// <summary>
        /// Gets a headers values by name.
        /// </summary>
        /// <param name="name">The name of the header.</param>
        /// <returns>The header values.</returns>
        IReadOnlyCollection<string> this[string name] { get; }
    }
}
