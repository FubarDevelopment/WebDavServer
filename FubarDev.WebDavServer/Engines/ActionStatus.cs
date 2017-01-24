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
    }
}
