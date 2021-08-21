using System;
using System.Threading.Tasks;
using AustinHarris.JsonRpc;
using GitDatabaseMerger.Interop;

namespace GitDatabaseMerger
{
    public class MergeAPI : JsonRpcService
    {
        private IMerger Merger { get; }

        public MergeAPI(IMerger merger)
        {
            Merger = merger;
        }

        [JsonRpcMethod]
        public async Task<MergeResult> MergeAsync(string local, string remote, string ancestor)
        {
            return await Merger.MergeAsync(local, remote, ancestor);
        }
    }
}
