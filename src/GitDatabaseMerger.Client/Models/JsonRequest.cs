using GitDatabaseMerger.Client.Helpers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace GitDatabaseMerger.Client.Models
{
    public class JsonRequest
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "jsonrpc")]
        public string jsonrpc = "2.0";
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "method")]
        public string method = "";
        [JsonProperty(NullValueHandling = NullValueHandling.Include, PropertyName = "params")]
        public List<object> parameters = new List<object>();
        [JsonProperty(NullValueHandling = NullValueHandling.Include, PropertyName = "id")]
        public int id = 1;

        public JsonRequest(string method, params object[] parameters)
        {
            this.method = method;
            this.parameters = parameters.ToList();
        }

        public override string ToString()
        {
            return this.Serialize();
        }
    }
}
