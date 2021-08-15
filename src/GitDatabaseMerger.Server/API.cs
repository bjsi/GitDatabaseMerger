using System;
using AustinHarris.JsonRpc;

namespace GitDatabaseMerger
{
    public enum Result
    {
        Success = 0,
        FailedMergeConflict = 1,
        FailedAborted = 2,
    }

    public class MergeAPI : JsonRpcService
    {
        [JsonRpcMethod]
        public string Ping()
        {
            return "pong";
        }

        [JsonRpcMethod]
        public Result Merge(string local, string remote, string ancestor)
        {
            Console.WriteLine($"Local: {local}");
            Console.WriteLine($"Remote: {remote}");
            Console.WriteLine($"Ancestor: {ancestor}");
            return Result.Success;
        }
    }
}
