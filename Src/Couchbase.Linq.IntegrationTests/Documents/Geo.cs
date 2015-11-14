using Newtonsoft.Json;

namespace Couchbase.Linq.IntegrationTests.Documents
{
    public class Geo
    {
        [JsonProperty("accuracy")]
        public virtual string Accuracy { get; set; }

        [JsonProperty("lat")]
        public virtual decimal Latitude { get; set; }

        [JsonProperty("lon")]
        public virtual decimal Longitude { get; set; }
    }
}