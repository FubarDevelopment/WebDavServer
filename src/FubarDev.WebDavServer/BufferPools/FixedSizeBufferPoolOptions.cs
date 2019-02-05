// <copyright file="FixedSizeBufferPoolOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.BufferPools
{
    /// <summary>
    /// The options for the <see cref="FixedSizeBufferPool"/>.
    /// </summary>
    public class FixedSizeBufferPoolOptions
    {
        /// <summary>
        /// Gets or sets the requested size for the returned buffer.
        /// </summary>
        /// <remarks>
        /// Uses the value of <see cref="FixedSizeBufferPool.DefaultBufferSize"/> if not set.
        /// </remarks>
        public int? Size { get; set; }
    }
}
