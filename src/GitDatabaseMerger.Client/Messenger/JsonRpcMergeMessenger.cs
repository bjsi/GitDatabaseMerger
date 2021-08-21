using GitDatabaseMerger.Client.Models;
using GitDatabaseMerger.Interop;
using System.Threading.Tasks;

namespace GitDatabaseMerger.Client.Messenger
{
    public class JsonRpcMergeMessenger
    {
        private JsonRpcMessenger Messenger { get; }
        public JsonRpcMergeMessenger(string hostname, int port)
        {
            Messenger = new JsonRpcMessenger(hostname, port);
        }

        public async Task<MergeResult> MergeAsync(string local, string remote, string ancestor)
        {
            var ret = await Messenger.SendRequestAsync<MergeResponse>("Merge", local, remote, ancestor);
            return ret == null
                ? MergeResult.FailedWithAbort
                : ret.Result;
        }
    }
}
