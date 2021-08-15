using GitDatabaseMerger.Client.Models;
using GitDatabaseMerger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitDatabaseMerger.Client.Messenger
{
    public class GitMessenger
    {
        private JsonMessenger Messenger { get; } = new JsonMessenger();
        public MergeResult Merge(string local, string remote, string ancestor)
        {
            var ret = Messenger.SendRequest<GitMergeResponse>("Merge", local, remote, ancestor);
            return ret == null
                ? MergeResult.FailedToConnect
                : ret.Result;
        }
    }
}
