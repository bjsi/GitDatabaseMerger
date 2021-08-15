using GitDatabaseMerger.Client.Helpers;
using Newtonsoft.Json;

namespace GitDatabaseMerger.Client.Models
{ 
  /// <summary>A JSON Response.</summary>
  public abstract class JsonResponse
  {
    [JsonProperty("jsonrpc", Required = Required.Always)]
    public string Version;

    /// <summary>Unique Request Id.</summary>
    [JsonProperty("id", Required = Required.Always)]
    public int Id;

    /// <summary>The error. NULL if no error occured.</summary>
    [JsonProperty("error", Required = Required.Default)]
    public string Error;

    public override string ToString()
    {
      return this.Serialize();
    }

    public bool ShouldSerializeError()
    {
      return this.Error != null;
    }
  }
}
