namespace GitDatabaseMerger.Interop
{
    public enum MergeResult
    {
        Success = 0,
        FailedWithUnresolvedConflict = 1,
        FailedWithAbort = 2,
    }
}
