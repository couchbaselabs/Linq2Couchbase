using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Couchbase.Linq.IntegrationTests.Documents
{
    [Filters.DocumentTypeFilter("beer")]
    public class BeerFiltered
    {
        [JsonProperty("name")]
        public virtual string Name { get; set; }

        [JsonProperty("abv")]
        public virtual decimal Abv { get; set; }

        [JsonProperty("ibu")]
        public virtual decimal Ibu { get; set; }

        [JsonProperty("srm")]
        public decimal Srm { get; set; }

        [JsonProperty("upc")]
        public virtual decimal Upc { get; set; }

        [JsonProperty("type")]
        public virtual string Type { get; set; }

        [JsonProperty("brewery_id")]
        public virtual string BreweryId { get; set; }

        [JsonProperty("description")]
        public virtual string Description { get; set; }

        [JsonProperty("style")]
        public virtual string Style { get; set; }

        [JsonProperty("category")]
        public virtual string Category { get; set; }

        [JsonProperty("updated")]
        public virtual DateTime Updated { get; set; }

        [Key]
        public string Key { get { return BreweryId + "-" + Name.ToLower().Replace(' ', '_'); } }
    }
}