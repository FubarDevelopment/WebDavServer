// <copyright file="IfModifiedSinceHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.WebDavServer.Props.Converters;

namespace FubarDev.WebDavServer.Model.Headers
{
    /// <summary>
    /// Class that represents the HTTP <code>If-Modified-Since</code> header
    /// </summary>
    public class IfModifiedSinceHeader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IfModifiedSinceHeader"/> class.
        /// </summary>
        /// <param name="lastWriteTimeUtc">The last time the defination was changed</param>
        public IfModifiedSinceHeader(DateTime lastWriteTimeUtc)
        {
            LastWriteTimeUtc = lastWriteTimeUtc;
        }

        /// <summary>
        /// Gets the last time the definition was changed
        /// </summary>
        public DateTime LastWriteTimeUtc { get; }

        /// <summary>
        /// Parses the header string to get a new instance of the <see cref="IfModifiedSinceHeader"/> class
        /// </summary>
        /// <param name="s">The header string to parse</param>
        /// <returns>The new instance of the <see cref="IfModifiedSinceHeader"/> class</returns>
        public static IfModifiedSinceHeader Parse(string s)
        {
            return new IfModifiedSinceHeader(DateTimeRfc1123Converter.Parse(s));
        }

        /// <summary>
        /// Returns a value that indicates whether the <paramref name="lastWriteTimeUtc"/> is past the value in the <code>If-Modified-Since</code> header
        /// </summary>
        /// <param name="lastWriteTimeUtc">The last write time of the entry to compare with</param>
        /// <returns><see langref="true"/> when the <paramref name="lastWriteTimeUtc"/> is past the value in the <code>If-Modified-Since</code> header</returns>
        public bool IsMatch(DateTime lastWriteTimeUtc)
        {
            return lastWriteTimeUtc > LastWriteTimeUtc;
        }
    }
}
