using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Couchbase.Linq.IntegrationTests.Documents
{
    public class Brewery
    {
        [JsonProperty("name")]
        public virtual string Name { get; set; }

        [JsonProperty("city")]
        public virtual string City { get; set; }

        [JsonProperty("state")]
        public virtual string State { get; set; }

        [JsonProperty("code")]
        public virtual string Code { get; set; }

        [JsonProperty("country")]
        public virtual string Country { get; set; }

        [JsonProperty("phone")]
        public virtual string Phone { get; set; }

        [JsonProperty("website")]
        public virtual string Website { get; set; }

        [JsonProperty("type")]
        public virtual string Type { get; set; }

        [JsonProperty("updated")]
        public virtual DateTime Updated { get; set; }

        [JsonProperty("description")]
        public virtual string Description { get; set; }

        [JsonProperty("address")]
        public virtual IList<string> Address { get; set; }

        [JsonProperty("geo")]
        public virtual Geo Geo { get; set; }

        /// <summary>
        /// Note: This property doesn't exist in the default beer-sample.  For tests we're acting as if it exists,
        /// and is a list of keys for all beers made by the brewery.
        /// </summary>
        [JsonProperty("beers")]
        public virtual IList<string> Beers { get; set; }
    }
}