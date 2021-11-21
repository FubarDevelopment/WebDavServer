// <copyright file="IWebDavResponse.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.IO;

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// Access to all elements that needs to be set during a WebDAV response
    /// </summary>
    public interface IWebDavResponse
    {
        /// <summary>
        /// Gets the dispatcher that handles all WebDAV requests.
        /// </summary>
        IWebDavContext Context { get; }

        /// <summary>
        /// Gets the HTTP response headers.
        /// </summary>
        IDictionary<string, string[]> Headers { get; }

        /// <summary>
        /// Gets or sets the content type.
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// Gets the response body stream.
        /// </summary>
        Stream Body { get; }
    }
}
