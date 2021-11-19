// <copyright file="ArrayPoolBufferPool.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Buffers;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.BufferPools
{
    /// <summary>
    /// A buffer pool implementation based on <see cref="ArrayPool{T}"/>.
    /// </summary>
    internal class ArrayPoolBufferPool : IBufferPool
    {
        private readonly ArrayPool<byte> _arrayPool;

        private readonly ILogger _logger;

        private readonly TimeSpan _maxDelay = TimeSpan.FromMilliseconds(200);

        private readonly Stopwatch _stopwatch = new Stopwatch();

        private readonly int _maxBufferSize = 0x4000000;

        private int _bufferSize = FixedSizeBufferPool.DefaultBufferSize;

        private byte[] _buffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayPoolBufferPool"/> class.
        /// </summary>
        /// <param name="arrayPool">The array pool to use.</param>
        /// <param name="logger">The logger to be used.</param>
        public ArrayPoolBufferPool(ArrayPool<byte> arrayPool, ILogger logger)
        {
            _arrayPool = arrayPool;
            _logger = logger;
            _buffer = _arrayPool.Rent(_bufferSize);
        }

        /// <inheritdoc />
        public byte[] GetBuffer(int readCount)
        {
            var elapsed = _stopwatch.Elapsed;
            if (readCount == _bufferSize && elapsed < _maxDelay && _bufferSize < _maxBufferSize)
            {
                _arrayPool.Return(_buffer);
                _bufferSize *= 2;
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("Increased buffer size to {NewBufferSize}", _bufferSize);
                }

                _buffer = _arrayPool.Rent(_bufferSize);
            }

            _stopwatch.Restart();
            return _buffer;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _arrayPool.Return(_buffer);
            _buffer = Array.Empty<byte>();
        }
    }
}
