// <copyright file="IWebDavClass.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace FubarDev.WebDavServer.Dispatchers
{
    /// <summary>
    /// Base interface for all WebDAV class implementations.
    /// </summary>
    public interface IWebDavClass
    {
        /// <summary>
        /// Gets the HTTP methods supported by this WebDAV class.
        /// </summary>
        IEnumerable<string> HttpMethods { get; }

        /// <summary>
        /// Gets the context for a request.
        /// </summary>
        IWebDavContext WebDavContext { get; }

        /// <summary>
        /// Gets the headers to be sent for a response of an <c>OPTIONS</c> request.
        /// </summary>
        IReadOnlyDictionary<string, IEnumerable<string>> OptionsResponseHeaders { get; }

        /// <summary>
        /// Gets the headers to be sent for any response.
        /// </summary>
        IReadOnlyDictionary<string, IEnumerable<string>> DefaultResponseHeaders { get; }
    }
}
