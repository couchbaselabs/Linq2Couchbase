
using Newtonsoft.Json;

namespace Couchbase.Linq.UnitTests.Documents
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

    /*"day": 0,
      "utc": "22:03:00",
      "flight": "AA138"
     * */
}
