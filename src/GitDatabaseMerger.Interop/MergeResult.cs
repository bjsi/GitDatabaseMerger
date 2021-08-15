using System;

namespace GitDatabaseMerger.Interop
{
    public enum MergeResult
    {
        Success = 0,
        FailedWithConflict = 1,
        FailedWithAbort = 2,
        FailedToConnect = 3,
    }
}
