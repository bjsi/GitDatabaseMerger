using GitDatabaseMerger.Client.Models;
using GitDatabaseMerger.Interop;

namespace GitDatabaseMerger.Client.Messenger
{
    public class JsonRpcMergeMessenger
    {
        private JsonRpcMessenger Messenger { get; }
        public JsonRpcMergeMessenger(string hostname, int port)
        {
            Messenger = new JsonRpcMessenger(hostname, port);
        }

        public MergeResult Merge(string local, string remote, string ancestor)
        {
            var ret = Messenger.SendRequest<MergeResponse>("Merge", local, remote, ancestor);
            return ret == null
                ? MergeResult.FailedWithAbort
                : ret.Result;
        }
    }
}
