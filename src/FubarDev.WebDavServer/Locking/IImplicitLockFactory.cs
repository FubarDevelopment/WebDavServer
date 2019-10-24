// <copyright file="IImplicitLockFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// Interface for an implicit lock factory.
    /// </summary>
    /// <remarks>
    /// Implicit locks are used during several operations
    /// like <c>PROPPATCH</c>, <c>MKCOL</c>, <c>DELETE</c>,
    /// <c>PUT</c>, <c>COPY</c>, and <c>MOVE</c>.
    /// </remarks>
    public interface IImplicitLockFactory
    {
        /// <summary>
        /// Create a new implicit lock.
        /// </summary>
        /// <param name="lockRequirements">The lock requirements.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A new instance of the created implicit lock.</returns>
        Task<IImplicitLock> CreateAsync(ILock? lockRequirements, CancellationToken cancellationToken);
    }
}
