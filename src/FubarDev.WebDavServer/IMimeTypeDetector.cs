// <copyright file="IMimeTypeDetector.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// Interface to be implemented by a mime type detector
    /// </summary>
    public interface IMimeTypeDetector
    {
        /// <summary>
        /// Tries to detect the mime type for a given <paramref name="entry"/>
        /// </summary>
        /// <param name="entry">The <see cref="IEntry"/> to detect the mime type for</param>
        /// <param name="mimeType">The detected mime type</param>
        /// <returns><c>true</c> when the mime type could be detected</returns>
        bool TryDetect(IEntry entry, out string mimeType);
    }
}
