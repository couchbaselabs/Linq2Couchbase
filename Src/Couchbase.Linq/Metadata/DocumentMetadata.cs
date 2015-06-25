using Newtonsoft.Json;

namespace Couchbase.Linq.Metadata
{
    /// <summary>
    /// Metadata about a document in Couchbase
    /// </summary>
    public class DocumentMetadata
    {

        [JsonProperty("cas")]
        public double Cas { get; set; }

        [JsonProperty("flags")]
        public int Flags { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }
}
