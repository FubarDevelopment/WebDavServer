// <copyright file="SQLiteLockManagerOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Locking.SQLite
{
    /// <summary>
    /// Options for the <see cref="SQLiteLockManager"/>
    /// </summary>
    public class SQLiteLockManagerOptions : ILockManagerOptions
    {
        /// <summary>
        /// Gets or sets the file name of the SQLite database
        /// </summary>
        public string DatabaseFileName { get; set; }

        /// <inheritdoc />
        public ILockTimeRounding Rounding { get; set; } = new DefaultLockTimeRounding(DefaultLockTimeRoundingMode.OneSecond);
    }
}
