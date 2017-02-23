// <copyright file="ActionStatus.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Engines
{
    /// <summary>
    /// The status of an action
    /// </summary>
    public enum ActionStatus
    {
        /// <summary>
        /// The target entry was created
        /// </summary>
        Created,

        /// <summary>
        /// The target entry was overwritten
        /// </summary>
        Overwritten,

        /// <summary>
        /// The target entry couldn't be overwritten, because a precondition failed.
        /// </summary>
        CannotOverwrite,

        /// <summary>
        /// Creating the target entry failed
        /// </summary>
        CreateFailed,

        /// <summary>
        /// The cleanup failed after a collection was processed
        /// </summary>
        CleanupFailed,

        /// <summary>
        /// Setting the property/properties failed
        /// </summary>
        PropSetFailed,

        /// <summary>
        /// An operation on a parent entry failed
        /// </summary>
        ParentFailed,

        /// <summary>
        /// Deleting the target element failed
        /// </summary>
        TargetDeleteFailed,

        /// <summary>
        /// Overwrite operation failed
        /// </summary>
        OverwriteFailed,

        /// <summary>
        /// The entry was ignored
        /// </summary>
        Ignored,
    }
}
