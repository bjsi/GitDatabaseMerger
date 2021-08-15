using GitDatabaseMerger.Interop;
using Newtonsoft.Json;

namespace GitDatabaseMerger.Client.Models
{
    public class GitMergeResponse : JsonResponse
    {
        /// <summary>The result of the merge.</summary>
        [JsonProperty("result", Required = Required.Default)]
        public MergeResult Result;
    }
}
