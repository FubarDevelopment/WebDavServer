// <copyright file="IGetCollectionHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Handlers
{
    /// <summary>
    /// Interface that provides the <c>GET</c> handler for a collection.
    /// </summary>
    public interface IGetCollectionHandler
    {
        /// <summary>
        /// Gets the collection contents.
        /// </summary>
        /// <param name="collection">The collection to get the contents for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The stream with the contents of the collection.</returns>
        Task<Stream> GetCollectionAsync(ICollection collection, CancellationToken cancellationToken);
    }
}
