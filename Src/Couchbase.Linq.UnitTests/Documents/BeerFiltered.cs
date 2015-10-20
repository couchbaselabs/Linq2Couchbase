using System;
using Newtonsoft.Json;

namespace Couchbase.Linq.UnitTests.Documents
{
    [Filters.DocumentTypeFilter("beer")]
    public class BeerFiltered
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
    }
}