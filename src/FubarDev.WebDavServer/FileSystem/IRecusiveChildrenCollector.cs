// <copyright file="IRecusiveChildrenCollector.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem
{
    /// <summary>
    /// A collection implementing this interface allows a more efficient collection of child entries
    /// </summary>
    public interface IRecusiveChildrenCollector
    {
        /// <summary>
        /// Gets all child entries up to the depth of <paramref name="maxDepth"/>
        /// </summary>
        /// <param name="maxDepth">The maximum depth (<see cref="int.MaxValue"/> as infinity)</param>
        /// <returns>All found child entries</returns>
        [NotNull]
        IAsyncEnumerable<IEntry> GetEntries(int maxDepth);
    }
}
