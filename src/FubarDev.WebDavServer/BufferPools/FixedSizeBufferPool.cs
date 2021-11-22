// <copyright file="FixedSizeBufferPool.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

#if !DEBUG
using FubarDev.WebDavServer.Utils;
#endif

namespace FubarDev.WebDavServer.BufferPools
{
    /// <summary>
    /// A <see cref="IBufferPool"/> implementation that returns the same buffer of the configured size.
    /// </summary>
    internal class FixedSizeBufferPool : IBufferPool
    {
#if DEBUG
        public const int DefaultBufferSize = 4096;
#else
        public const int DefaultBufferSize = SystemInfo.CopyBufferSize;
#endif

        private readonly byte[] _buffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedSizeBufferPool"/> class.
        /// </summary>
        /// <param name="options">The options for this buffer pool.</param>
        public FixedSizeBufferPool(FixedSizeBufferPoolOptions options)
        {
            _buffer = new byte[options.Size ?? DefaultBufferSize];
        }

        /// <inheritdoc />
        public byte[] GetBuffer(int readCount)
        {
            return _buffer;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Nothing to dispose here.
        }
    }
}
