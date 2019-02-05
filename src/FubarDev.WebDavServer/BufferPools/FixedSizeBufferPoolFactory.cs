// <copyright file="FixedSizeBufferPoolFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.BufferPools
{
    /// <summary>
    /// Buffer pool factory implementation returning new <see cref="FixedSizeBufferPool"/> objects.
    /// </summary>
    public class FixedSizeBufferPoolFactory : IBufferPoolFactory
    {
        private readonly IOptions<FixedSizeBufferPoolOptions> _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedSizeBufferPoolFactory"/> class.
        /// </summary>
        /// <param name="options">The options for this factory.</param>
        public FixedSizeBufferPoolFactory(IOptions<FixedSizeBufferPoolOptions> options)
        {
            _options = options;
        }

        /// <inheritdoc />
        public IBufferPool CreatePool()
        {
            return new FixedSizeBufferPool(_options.Value);
        }
    }
}
