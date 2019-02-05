// <copyright file="ArrayPoolBufferPoolFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Buffers;

using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.BufferPools
{
    /// <summary>
    /// The factory to create <see cref="ArrayPoolBufferPool"/> instances.
    /// </summary>
    public class ArrayPoolBufferPoolFactory : IBufferPoolFactory
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayPoolBufferPoolFactory"/> class.
        /// </summary>
        /// <param name="logger">The logger to be used by the buffer pool.</param>
        public ArrayPoolBufferPoolFactory(ILogger<ArrayPoolBufferPoolFactory> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public IBufferPool CreatePool()
        {
            return new ArrayPoolBufferPool(ArrayPool<byte>.Shared, _logger);
        }
    }
}
