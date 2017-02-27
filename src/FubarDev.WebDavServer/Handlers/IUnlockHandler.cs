// <copyright file="IUnlockHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model.Headers;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers
{
    /// <summary>
    /// Interface for the <c>UNLOCK</c> handler
    /// </summary>
    public interface IUnlockHandler : IClass2Handler
    {
        /// <summary>
        /// Removes a LOCK with the given <paramref name="stateToken"/> from the given <paramref name="path"/>
        /// </summary>
        /// <param name="path">The path to remove the lock for</param>
        /// <param name="stateToken">The state token of the lock to remove</param>
        /// <param name="cancellationToken">The cancellcation token</param>
        /// <returns>The result of the operation</returns>
        [NotNull]
        [ItemNotNull]
        Task<IWebDavResult> UnlockAsync([NotNull] string path, [NotNull] LockTokenHeader stateToken, CancellationToken cancellationToken);
    }
}
