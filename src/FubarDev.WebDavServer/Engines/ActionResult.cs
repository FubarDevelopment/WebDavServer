// <copyright file="ActionResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines
{
    public class ActionResult
    {
        public ActionResult(ActionStatus status, [NotNull] ITarget target)
        {
            Status = status;
            Target = target;
            Href = target.DestinationUrl;
        }

        public ActionStatus Status { get; }

        [NotNull]
        public ITarget Target { get; }

        [NotNull]
        public Uri Href { get; set; }

        [CanBeNull]
        public Exception Exception { get; set; }

        [CanBeNull]
        [ItemNotNull]
        public IReadOnlyCollection<XName> FailedProperties { get; set; }

        public bool IsFailure => Status != ActionStatus.Created && Status != ActionStatus.Overwritten;
    }
}
