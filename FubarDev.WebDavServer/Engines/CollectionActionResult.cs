using System;
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
    }
}
