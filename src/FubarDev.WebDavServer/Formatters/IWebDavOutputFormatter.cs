﻿// <copyright file="IWebDavOutputFormatter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Text;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Formatters
{
    /// <summary>
    /// A formatter for WebDAV responses
    /// </summary>
    public interface IWebDavOutputFormatter
    {
        /// <summary>
        /// Gets the content type.
        /// </summary>
        [NotNull]
        string ContentType { get; }

        /// <summary>
        /// Gets the encoding.
        /// </summary>
        [NotNull]
        Encoding Encoding { get; }

        /// <summary>
        /// Serializes the <paramref name="data"/> to the <paramref name="output"/>.
        /// </summary>
        /// <typeparam name="T">The type of the <paramref name="data"/>.</typeparam>
        /// <param name="output">The stream to serialize to.</param>
        /// <param name="data">The data to serialize.</param>
        void Serialize<T>([NotNull] Stream output, T data);
    }
}
