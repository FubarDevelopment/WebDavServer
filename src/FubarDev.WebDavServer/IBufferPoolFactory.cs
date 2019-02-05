// <copyright file="IBufferPoolFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// Interface to create new buffer pools.
    /// </summary>
    public interface IBufferPoolFactory
    {
        /// <summary>
        /// Returns a new buffer pool.
        /// </summary>
        /// <returns>The new buffer pool.</returns>
        IBufferPool CreatePool();
    }
}
