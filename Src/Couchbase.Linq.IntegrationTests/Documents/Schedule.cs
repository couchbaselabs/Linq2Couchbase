using Newtonsoft.Json;

namespace Couchbase.Linq.IntegrationTests.Documents
{
    public class Schedule
    {
        [JsonProperty("day")]
        public uint Day { get; set; }

        [JsonProperty("utc")]
        public string Utc { get; set; }

        [JsonProperty("flight")]
        public string Flight { get; set; }
    }
}
