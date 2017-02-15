// <copyright file="ITimeoutPolicy.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// The policy to select the lock timeout
    /// </summary>
    public interface ITimeoutPolicy
    {
        /// <summary>
        /// Selects the timeout from the list of client requested timeouts.
        /// </summary>
        /// <param name="timeouts">The timeouts requested by the client</param>
        /// <returns>The timeout to use</returns>
        TimeSpan SelectTimeout([NotNull] IReadOnlyCollection<TimeSpan> timeouts);
    }
}
