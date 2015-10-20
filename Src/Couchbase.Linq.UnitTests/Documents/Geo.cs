using Newtonsoft.Json;

namespace Couchbase.Linq.UnitTests.Documents
{
    public class Geo
    {
        [JsonProperty("accuracy")]
        public string Accuracy { get; set; }

        [JsonProperty("lat")]
        public decimal Latitude { get; set; }

        [JsonProperty("lon")]
        public decimal Longitude { get; set; }
    }
}