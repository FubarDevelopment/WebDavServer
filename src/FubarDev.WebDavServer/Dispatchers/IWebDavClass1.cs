// <copyright file="IWebDavClass1.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Models;

namespace FubarDev.WebDavServer.Dispatchers
{
    /// <summary>
    /// Interface for WebDAV class 1 support.
    /// </summary>
    public interface IWebDavClass1 : IWebDavClass
    {
        /// <summary>
        /// Queries the options for a given path.
        /// </summary>
        /// <remarks>
        /// This is used to identify the WebDAV capabilities at a given URL.
        /// </remarks>
        /// <param name="path">The root-relative file system path to query the options for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        Task<IWebDavResult> OptionsAsync(string path, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the element at the given path.
        /// </summary>
        /// <param name="path">The path to the element to get.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        Task<IWebDavResult> GetAsync(string path, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the information about an element at the given path.
        /// </summary>
        /// <param name="path">The path to the element to get the information for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        Task<IWebDavResult> HeadAsync(string path, CancellationToken cancellationToken);

        /// <summary>
        /// Creates or updates a document at the given <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path where to create or update the document.</param>
        /// <param name="data">The data to write to the new or existing document.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        Task<IWebDavResult> PutAsync(string path, Stream data, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the element at the given path.
        /// </summary>
        /// <param name="path">The path to the element to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        Task<IWebDavResult> DeleteAsync(string path, CancellationToken cancellationToken);

        /// <summary>
        /// Queries properties (dead or live) for a given <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to query the properties for.</param>
        /// <param name="request">Some information about the properties to query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        Task<IWebDavResult> PropFindAsync(string path, propfind? request, CancellationToken cancellationToken);

        /// <summary>
        /// Patches (sets or removes) properties from the given. <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to patch the properties for.</param>
        /// <param name="request">The properties to patch.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        Task<IWebDavResult> PropPatchAsync(string path, propertyupdate request, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a collection at the given path.
        /// </summary>
        /// <param name="path">The path to the collection to create.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        Task<IWebDavResult> MkColAsync(string path, CancellationToken cancellationToken);

        /// <summary>
        /// Copies from the source to the destination.
        /// </summary>
        /// <param name="path">The source to copy.</param>
        /// <param name="destination">The destination to copy to.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        Task<IWebDavResult> CopyAsync(string path, Uri destination, CancellationToken cancellationToken);

        /// <summary>
        /// Moves from the source to the destination.
        /// </summary>
        /// <param name="path">The source to move.</param>
        /// <param name="destination">The destination to move to.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        Task<IWebDavResult> MoveAsync(string path, Uri destination, CancellationToken cancellationToken);
    }
}
