using System;

namespace GitDatabaseMerger.Interop
{
    public class MergeRequestEventArgs : EventArgs
    {
        public MergeRequestEventArgs(string local, string remote, string ancestor)
        {
            Local = local;
            Remote = remote;
            Ancestor = ancestor;
        }

        public string Local { get; }
        public string Remote { get; }
        public string Ancestor { get; }
    }
}
