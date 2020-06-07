using System;
using Newtonsoft.Json;

namespace Couchbase.Linq.IntegrationTests.Documents
{
    public class Beer
    {
        [JsonProperty("name")]
        public virtual string Name { get; set; }

        [JsonProperty("abv")]
        public virtual decimal Abv { get; set; }

        [JsonProperty("ibu")]
        public virtual decimal Ibu { get; set; }

        [JsonProperty("srm")]
        public virtual decimal Srm { get; set; }

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

        // TODO: Enabled UnixMillisecondsConverter once available https://issues.couchbase.com/browse/NCBC-2539
        // This property isn't normally on beers in the beer-sample bucket
        // But we need it for some integration tests so we'll add it
        //[JsonProperty("updatedUnixMillis")]
        //[JsonConverter(typeof(UnixMillisecondsConverter))]
        //public virtual DateTime? UpdatedUnixMillis { get; set; }
    }
}