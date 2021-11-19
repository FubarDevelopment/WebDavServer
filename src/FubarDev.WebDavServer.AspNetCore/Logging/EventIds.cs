// <copyright file="EventIds.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.AspNetCore.Logging
{
    /// <summary>
    /// The default logging event IDs.
    /// </summary>
    public static class EventIds
    {
        /// <summary>
        /// Gets the unspecified event ID.
        /// </summary>
        public static EventId Unspecified { get; } = new EventId(0);
    }
}
