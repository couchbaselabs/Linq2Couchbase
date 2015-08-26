using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Couchbase.Linq.Tests.Documents
{
    public class Brewery
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("updated")]
        public DateTime Updated { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("address")]
        public List<string> Address { get; set; }

        [JsonProperty("geo")]
        public Geo Geo { get; set; }

        /// <summary>
        /// Note: This property doesn't exist in the default beer-sample.  For tests we're acting as if it exists,
        /// and is a list of keys for all beers made by the brewery.
        /// </summary>
        [JsonProperty("beers")]
        public List<string> Beers { get; set; }
    }
}