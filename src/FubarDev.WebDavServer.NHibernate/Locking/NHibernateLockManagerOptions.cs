// <copyright file="NHibernateLockManagerOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.Locking;

namespace FubarDev.WebDavServer.NHibernate.Locking
{
    /// <summary>
    /// Options for the <see cref="NHibernateLockManager"/>
    /// </summary>
    public class NHibernateLockManagerOptions : ILockManagerOptions
    {
        /// <inheritdoc />
        public ILockTimeRounding Rounding { get; set; } = new DefaultLockTimeRounding(DefaultLockTimeRoundingMode.OneSecond);
    }
}
