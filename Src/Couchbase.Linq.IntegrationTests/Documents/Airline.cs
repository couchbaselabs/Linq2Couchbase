using Newtonsoft.Json;

namespace Couchbase.Linq.IntegrationTests.Documents
{
    /*
     * {
          "id": 24,
          "type": "airline",
          "name": "American Airlines",
          "iata": "AA",
          "icao": "AAL",
          "callsign": "AMERICAN",
          "country": "United States"
        }
     * */
    public class Airline
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("iata")]
        public string Iata { get; set; }

        [JsonProperty("icao")]
        public string Icao { get; set; }

        [JsonProperty("callsign")]
        public string Callsign { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }
}
