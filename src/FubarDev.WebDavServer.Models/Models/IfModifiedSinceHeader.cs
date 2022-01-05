// <copyright file="IfModifiedSinceHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Models
{
    /// <summary>
    /// Class that represents the HTTP <c>If-Modified-Since</c> header.
    /// </summary>
    public class IfModifiedSinceHeader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IfModifiedSinceHeader"/> class.
        /// </summary>
        /// <param name="lastWriteTimeUtc">The last time the definition was changed.</param>
        public IfModifiedSinceHeader(DateTimeOffset lastWriteTimeUtc)
        {
            LastWriteTimeUtc = lastWriteTimeUtc;
        }

        /// <summary>
        /// Gets the last time the definition was changed.
        /// </summary>
        public DateTimeOffset LastWriteTimeUtc { get; }

        /// <summary>
        /// Parses the header string to get a new instance of the <see cref="IfModifiedSinceHeader"/> class.
        /// </summary>
        /// <param name="s">The header string to parse.</param>
        /// <returns>The new instance of the <see cref="IfModifiedSinceHeader"/> class.</returns>
        public static IfModifiedSinceHeader Parse(string s)
        {
            return new IfModifiedSinceHeader(WebDavXml.ParseRfc1123(s));
        }

        /// <summary>
        /// Returns a value that indicates whether the <paramref name="lastWriteTimeUtc"/> is past the value in the <c>If-Modified-Since</c> header.
        /// </summary>
        /// <param name="lastWriteTimeUtc">The last write time of the entry to compare with.</param>
        /// <returns><see langword="true"/> when the <paramref name="lastWriteTimeUtc"/> is past the value in the <c>If-Modified-Since</c> header.</returns>
        public bool IsMatch(DateTime lastWriteTimeUtc)
        {
            return lastWriteTimeUtc > LastWriteTimeUtc;
        }
    }
}
