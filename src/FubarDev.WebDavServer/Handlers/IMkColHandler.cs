// <copyright file="IMkColHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers
{
    /// <summary>
    /// Interface for the <code>MKCOL</code> handler
    /// </summary>
    public interface IMkColHandler : IClass1Handler
    {
        /// <summary>
        /// Creates a collection at the given path
        /// </summary>
        /// <param name="path">The path to the collection to create</param>
        /// <param name="cancellationToken">The cancellcation token</param>
        /// <returns>The result of the operation</returns>
        [NotNull]
        [ItemNotNull]
        Task<IWebDavResult> MkColAsync([NotNull] string path, CancellationToken cancellationToken);
    }
}
