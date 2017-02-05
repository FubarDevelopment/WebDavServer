// <copyright file="CollectionActionResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace FubarDev.WebDavServer.Engines
{
    public class CollectionActionResult : ActionResult
    {
        public CollectionActionResult(ActionStatus status, ITarget target)
            : base(status, target)
        {
        }

        public IReadOnlyCollection<ActionResult> DocumentActionResults { get; set; }

        public IReadOnlyCollection<CollectionActionResult> CollectionActionResults { get; set; }

        public IEnumerable<ActionResult> Flatten()
        {
            return Flatten(this);
        }

        private static IEnumerable<ActionResult> Flatten(CollectionActionResult collectionResult)
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
