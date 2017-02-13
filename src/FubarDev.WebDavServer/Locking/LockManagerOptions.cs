// <copyright file="LockManagerOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// The options for the <see cref="ILockManager"/>
    /// </summary>
    public class LockManagerOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockManagerOptions"/> class.
        /// </summary>
        public LockManagerOptions()
        {
            Rounding = new DefaultLockTimeRounding(DefaultLockTimeRoundingMode.OneSecond);
        }

        /// <summary>
        /// Gets or sets the time rounding implementation
        /// </summary>
        public ILockTimeRounding Rounding { get; set; }
    }
}
