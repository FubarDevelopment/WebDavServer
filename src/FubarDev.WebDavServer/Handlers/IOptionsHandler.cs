// <copyright file="IOptionsHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers
{
    /// <summary>
    /// Interface for the <code>OPTIONS</code> handler
    /// </summary>
    public interface IOptionsHandler : IClass1Handler
    {
        /// <summary>
        /// Queries the options for a given path.
        /// </summary>
        /// <remarks>
        /// This is used to identify the WebDAV capabilities at a given URL.
        /// </remarks>
        /// <param name="path">The root-relataive file system path to query the options for</param>
        /// <param name="cancellationToken">The cancellcation token</param>
        /// <returns>The result of the operation</returns>
        [NotNull]
        [ItemNotNull]
        Task<IWebDavResult> OptionsAsync([NotNull] string path, CancellationToken cancellationToken);
    }
}
