// <copyright file="IHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers
{
    /// <summary>
    /// The handler for a HTTP method for a given WebDAV class
    /// </summary>
    public interface IHandler
    {
        /// <summary>
        /// Gets the HTTP methods that are processed by this handler.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        IEnumerable<string> HttpMethods { get; }
    }
}
