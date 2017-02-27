// <copyright file="IPutHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers
{
    /// <summary>
    /// Interface for the <c>PUT</c> handler
    /// </summary>
    public interface IPutHandler : IClass1Handler
    {
        /// <summary>
        /// Creates or updates a document at the given <paramref name="path"/>
        /// </summary>
        /// <param name="path">The path where to create or update the document</param>
        /// <param name="data">The data to write to the new or existing document</param>
        /// <param name="cancellationToken">The cancellcation token</param>
        /// <returns>The result of the operation</returns>
        [NotNull]
        [ItemNotNull]
        Task<IWebDavResult> PutAsync([NotNull] string path, [NotNull] Stream data, CancellationToken cancellationToken);
    }
}
