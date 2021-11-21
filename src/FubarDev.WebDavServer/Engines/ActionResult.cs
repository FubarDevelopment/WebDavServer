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
    /// <param name="Status">The status of the action.</param>
    /// <param name="Target">The element this status is for.</param>
    public record ActionResult(ActionStatus Status, ITarget Target)
    {
        /// <summary>
        /// Gets or sets the destination URL for the <see cref="Target"/>.
        /// </summary>
        public Uri Href { get; set; } = Target.DestinationUrl;

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
