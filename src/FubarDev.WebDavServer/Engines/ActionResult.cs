// <copyright file="ActionResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.Engines
{
    /// <summary>
    /// The result of an action.
    /// </summary>
    public class ActionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionResult"/> class.
        /// </summary>
        /// <param name="status">The status of the action.</param>
        /// <param name="target">The element this status is for.</param>
        public ActionResult(ActionStatus status, ITarget target)
        {
            Status = status;
            Target = target;
            Href = target.DestinationUrl;
        }

        /// <summary>
        /// Gets the status of the action.
        /// </summary>
        public ActionStatus Status { get; }

        /// <summary>
        /// Gets the target entry this action status is for.
        /// </summary>
        public ITarget Target { get; }

        /// <summary>
        /// Gets or sets the destination URL for the <see cref="Target"/>.
        /// </summary>
        public Uri Href { get; set; }

        /// <summary>
        /// Gets or sets the exception that occurred during the execution of the action.
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// Gets or sets the names of properties that couldn't be set.
        /// </summary>
        public IReadOnlyCollection<XName>? FailedProperties { get; set; }

        /// <summary>
        /// Gets a value indicating whether the action failed.
        /// </summary>
        public bool IsFailure => Status != ActionStatus.Created && Status != ActionStatus.Overwritten;
    }
}
