// <copyright file="IWebDavContextAccessor.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// Service used to access the WebDAV context.
    /// </summary>
    public interface IWebDavContextAccessor
    {
        /// <summary>
        /// Gets the WebDAV context.
        /// </summary>
        IWebDavContext WebDavContext { get; }
    }
}
