using System;
using Newtonsoft.Json;

namespace Couchbase.Linq.Tests.Documents
{
    public class Beer
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("abv")]
        public decimal Abv { get; set; }

        [JsonProperty("ibu")]
        public decimal Ibu { get; set; }

        [JsonProperty("srm")]
        public decimal Srm { get; set; }

        [JsonProperty("upc")]
        public decimal Upc { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("brewery_id")]
        public string BreweryId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("style")]
        public string Style { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("updated")]
        public DateTime Updated { get; set; }
        /*
         * {
              "name": "21A IPA",
              "abv": 7.2,
              "ibu": 0,
              "srm": 0,
              "upc": 0,
              "type": "beer",
              "brewery_id": "21st_amendment_brewery_cafe",
              "updated": "2010-07-22 20:00:20",
              "description": "Deep golden color. Citrus and piney hop aromas. Assertive malt backbone supporting the overwhelming bitterness. Dry hopped in the fermenter with four types of hops giving an explosive hop aroma. Many refer to this IPA as Nectar of the Gods. Judge for yourself. Now Available in Cans!",
              "style": "American-Style India Pale Ale",
              "category": "North American Ale"
            }
         */
    }
}
