// <copyright file="EventIds.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.AspNetCore.Logging
{
    /// <summary>
    /// The default logging event IDs
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "EventId is a struct.")]
    public static class EventIds
    {
        /// <summary>
        /// The unspecified event ID
        /// </summary>
        public static EventId Unspecified = new EventId(0);
    }
}
