using System.Collections.Generic;
using Newtonsoft.Json;

namespace Couchbase.Linq.IntegrationTests.Documents
{
    public class Route
    {
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("airline")]
        public string Airline { get; set; }

        [JsonProperty("airlineid")]
        public string AirlineId { get; set; }

        [JsonProperty("sourceairport")]
        public string SourceAirport { get; set; }

        [JsonProperty("destinationairport")]
        public string DestinationAirport { get; set; }

        [JsonProperty("stops")]
        public uint Stops { get; set; }

        [JsonProperty("equipment")]
        public string Equipment { get; set; }

        [JsonProperty("schedule")]
        public List<Schedule> Schedule { get; set; }
    }
 }
