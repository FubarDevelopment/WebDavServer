// <copyright file="IBufferPool.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// A pool to get byte array buffers for a PUT operation.
    /// </summary>
    public interface IBufferPool : IDisposable
    {
        /// <summary>
        /// Gets the buffer for the next block.
        /// </summary>
        /// <param name="readCount">The number of bytes read by the previous <see cref="Stream.Read(byte[], int, int)"/> operation.</param>
        /// <returns>The buffer to be used for the operation.</returns>
        byte[] GetBuffer(int readCount);
    }
}
