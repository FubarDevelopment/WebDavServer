// <copyright file="IWebDavDispatcher.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using FubarDev.WebDavServer.Dispatchers;
using FubarDev.WebDavServer.Formatters;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// The interface of a WebDAV server implementation
    /// </summary>
    public interface IWebDavDispatcher
    {
        /// <summary>
        /// Gets the list of supported WebDAV classes
        /// </summary>
        [NotNull]
        IReadOnlyCollection<string> SupportedClasses { get; }

        /// <summary>
        /// Gets the list of supported HTTP methods
        /// </summary>
        [NotNull]
        [ItemNotNull]
        IReadOnlyCollection<string> SupportedHttpMethods { get; }

        /// <summary>
        /// Gets the formatter for the WebDAV XML responses
        /// </summary>
        [NotNull]
        IWebDavOutputFormatter Formatter { get; }

        /// <summary>
        /// Gets the WebDAV class 1 implementation
        /// </summary>
        [NotNull]
        IWebDavClass1 Class1 { get; }

        /// <summary>
        /// Gets the WebDAV class 2 implementation
        /// </summary>
        [CanBeNull]
        IWebDavClass2 Class2 { get; }
    }
}
