// <copyright file="ILockManagerOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// The options for the <see cref="ILockManager"/>.
    /// </summary>
    public interface ILockManagerOptions
    {
        /// <summary>
        /// Gets or sets the time rounding implementation.
        /// </summary>
        ILockTimeRounding Rounding { get; set; }
    }
}
