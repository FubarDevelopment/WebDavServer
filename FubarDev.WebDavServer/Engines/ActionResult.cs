using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.Engines
{
    public class ActionResult
    {
        public ActionResult(ActionStatus status, ITarget target)
        {
            Status = status;
            Target = target;
            Href = target.DestinationUrl;
        }

        public ActionStatus Status { get; }
        public ITarget Target { get; }
        public Uri Href { get; set; }
        public Exception Exception { get; set; }
        public IReadOnlyCollection<XName> FailedProperties { get; set; }
        public bool IsFailure => Status != ActionStatus.Created && Status != ActionStatus.Overwritten;
    }
}
