// <copyright file="CollectionActionResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace FubarDev.WebDavServer.FileSystem
{
    /// <summary>
    /// Information about the success for a collection action.
    /// </summary>
    public class CollectionActionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionActionResult"/> class.
        /// </summary>
        /// <param name="target">The target to create the result information for.</param>
        /// <param name="createdChildEntries">The created child entries.</param>
        public CollectionActionResult(ICollection target, IReadOnlyCollection<IEntry> createdChildEntries)
            : this(target, createdChildEntries, null, WebDavStatusCode.OK)
        {
            Target = target;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionActionResult"/> class.
        /// </summary>
        /// <param name="target">The target to create the result information for.</param>
        /// <param name="createdChildEntries">The created child entries.</param>
        /// <param name="failedEntry">The failed child entry.</param>
        /// <param name="errorStatusCode">The status code for the failed child entry.</param>
        public CollectionActionResult(
            ICollection target,
            IReadOnlyCollection<IEntry> createdChildEntries,
            IEntry? failedEntry,
            WebDavStatusCode errorStatusCode)
        {
            Target = target;
            CreatedChildEntries = createdChildEntries;
            FailedEntry = failedEntry;
            ErrorStatusCode = errorStatusCode;
        }

        /// <summary>
        /// Gets the target this result information object is for.
        /// </summary>
        public ICollection Target { get; }

        /// <summary>
        /// Gets the created child entries.
        /// </summary>
        public IReadOnlyCollection<IEntry> CreatedChildEntries { get; }

        /// <summary>
        /// Gets the failed entry.
        /// </summary>
        public IEntry? FailedEntry { get; }

        /// <summary>
        /// Gets the status code for the failed entry.
        /// </summary>
        public WebDavStatusCode ErrorStatusCode { get; }
    }
}
