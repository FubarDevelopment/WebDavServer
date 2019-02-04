// <copyright file="ISystemClock.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// Interface for querying the system clock
    /// </summary>
    public interface ISystemClock
    {
        /// <summary>
        /// Gets the <see cref="DateTime.UtcNow"/>.
        /// </summary>
        DateTime UtcNow { get; }
    }
}
