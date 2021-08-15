using System;
using AustinHarris.JsonRpc;
using GitDatabaseMerger.Interop;

namespace GitDatabaseMerger
{
    public class MergeAPI : JsonRpcService
    {
        [JsonRpcMethod]
        public MergeResult Merge(string local, string remote, string ancestor)
        {
            Console.WriteLine($"Local: {local}");
            Console.WriteLine($"Remote: {remote}");
            Console.WriteLine($"Ancestor: {ancestor}");
            return MergeResult.Success;
        }
    }
}
