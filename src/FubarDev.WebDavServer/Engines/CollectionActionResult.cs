// <copyright file="CollectionActionResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines
{
    /// <summary>
    /// The result of an operation on a collection
    /// </summary>
    public class CollectionActionResult : ActionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionActionResult"/> class.
        /// </summary>
        /// <param name="status">The status of the operation</param>
        /// <param name="target">The target of the operation</param>
        public CollectionActionResult(ActionStatus status, ITarget target)
            : base(status, target)
        {
        }

        /// <summary>
        /// Gets or sets the action results of the documents of this collection
        /// </summary>
        public IReadOnlyCollection<ActionResult> DocumentActionResults { get; set; }

        /// <summary>
        /// Gets or sets the action results of the sub-collections of this collection
        /// </summary>
        public IReadOnlyCollection<CollectionActionResult> CollectionActionResults { get; set; }

        /// <summary>
        /// Returns a flat list of action results
        /// </summary>
        /// <remarks>
        /// This returns all action results for all sub-collections, documents and their child elements.
        /// </remarks>
        /// <returns>The flat list of action results</returns>
        public IEnumerable<ActionResult> Flatten()
        {
            return Flatten(this);
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<ActionResult> Flatten([NotNull] CollectionActionResult collectionResult)
        {
            yield return collectionResult;
            if (collectionResult.DocumentActionResults != null)
            {
                foreach (var result in collectionResult.DocumentActionResults)
                {
                    yield return result;
                }
            }

            if (collectionResult.CollectionActionResults != null)
            {
                foreach (var result in collectionResult.CollectionActionResults)
                {
                    yield return result;
                }
            }
        }
    }
}
