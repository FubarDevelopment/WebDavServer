// <copyright file="InMemoryLockManagerOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Locking.InMemory
{
    /// <summary>
    /// Options for the <see cref="InMemoryLockManager"/>
    /// </summary>
    public class InMemoryLockManagerOptions : ILockManagerOptions
    {
        /// <inheritdoc />
        public ILockTimeRounding Rounding { get; set; } = new DefaultLockTimeRounding(DefaultLockTimeRoundingMode.OneSecond);
    }
}
