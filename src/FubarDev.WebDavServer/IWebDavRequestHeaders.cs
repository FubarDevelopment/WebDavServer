// <copyright file="IWebDavRequestHeaders.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using FubarDev.WebDavServer.Model.Headers;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// Interface for the access to the request headers
    /// </summary>
    public interface IWebDavRequestHeaders
    {
        /// <summary>
        /// Gets the <c>Content-Length</c> header
        /// </summary>
        long? ContentLength { get; }

        /// <summary>
        /// Gets the <c>Depth</c> header
        /// </summary>
        DepthHeader? Depth { get; }

        /// <summary>
        /// Gets the value of the <c>Overwrite</c> (<see cref="OverwriteHeader"/>) header
        /// </summary>
        bool? Overwrite { get; }

        /// <summary>
        /// Gets the <c>If</c> header
        /// </summary>
        [CanBeNull]
        IfHeader If { get; }

        /// <summary>
        /// Gets the <c>If-Match</c> header
        /// </summary>
        [CanBeNull]
        IfMatchHeader IfMatch { get; }

        /// <summary>
        /// Gets the <c>If-None-Match</c> header
        /// </summary>
        [CanBeNull]
        IfNoneMatchHeader IfNoneMatch { get; }

        /// <summary>
        /// Gets the <c>If-Modified-Since</c> header
        /// </summary>
        [CanBeNull]
        IfModifiedSinceHeader IfModifiedSince { get; }

        /// <summary>
        /// Gets the <c>If-Unmodified-Since</c> header
        /// </summary>
        [CanBeNull]
        IfUnmodifiedSinceHeader IfUnmodifiedSince { get; }

        /// <summary>
        /// Gets the <c>Range</c> header
        /// </summary>
        [CanBeNull]
        RangeHeader Range { get; }

        /// <summary>
        /// Gets the <c>Timeout</c> header
        /// </summary>
        [CanBeNull]
        TimeoutHeader Timeout { get; }

        /// <summary>
        /// Gets all headers
        /// </summary>
        [NotNull]
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> Headers { get; }

        /// <summary>
        /// Gets a headers values by name
        /// </summary>
        /// <param name="name">The name of the header</param>
        /// <returns>The header values</returns>
        [NotNull]
        [ItemNotNull]
        IReadOnlyCollection<string> this[string name] { get; }
    }
}
