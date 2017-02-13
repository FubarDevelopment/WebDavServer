// <copyright file="ActionStatus.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Engines
{
    public enum ActionStatus
    {
        Created,
        Overwritten,
        CannotOverwrite,
        CreateFailed,
        CleanupFailed,
        PropSetFailed,
        ParentFailed,
        TargetDeleteFailed,
        OverwriteFailed,
        Ignored,
    }
}
