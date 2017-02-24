// <copyright file="IfUnmodifiedSinceHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.WebDavServer.Props.Converters;

namespace FubarDev.WebDavServer.Model.Headers
{
    /// <summary>
    /// Class that represents the HTTP <code>If-Unmodified-Since</code> header
    /// </summary>
    public class IfUnmodifiedSinceHeader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IfUnmodifiedSinceHeader"/> class.
        /// </summary>
        /// <param name="lastWriteTimeUtc">The last time the defination was changed</param>
        public IfUnmodifiedSinceHeader(DateTime lastWriteTimeUtc)
        {
            LastWriteTimeUtc = lastWriteTimeUtc;
        }

        /// <summary>
        /// Gets the last time the definition was changed
        /// </summary>
        public DateTime LastWriteTimeUtc { get; }

        /// <summary>
        /// Parses the header string to get a new instance of the <see cref="IfUnmodifiedSinceHeader"/> class
        /// </summary>
        /// <param name="s">The header string to parse</param>
        /// <returns>The new instance of the <see cref="IfUnmodifiedSinceHeader"/> class</returns>
        public static IfUnmodifiedSinceHeader Parse(string s)
        {
            return new IfUnmodifiedSinceHeader(DateTimeRfc1123Converter.Parse(s));
        }

        /// <summary>
        /// Returns a value that indicates whether the <paramref name="lastWriteTimeUtc"/> is not past the value in the <code>If-Unmodified-Since</code> header
        /// </summary>
        /// <param name="lastWriteTimeUtc">The last write time of the entry to compare with</param>
        /// <returns><see langref="true"/> when the <paramref name="lastWriteTimeUtc"/> is not past the value in the <code>If-Modified-Since</code> header</returns>
        public bool IsMatch(DateTime lastWriteTimeUtc)
        {
            return lastWriteTimeUtc <= LastWriteTimeUtc;
        }
    }
}
