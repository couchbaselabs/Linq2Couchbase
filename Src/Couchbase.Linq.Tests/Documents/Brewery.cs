using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Couchbase.Linq.Tests.Documents
{
    public class Brewery
    {
        public string Name { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Code { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        public DateTime Updated { get; set; }
        public string Description { get; set; }
        public List<string> Address { get; set; }
        public Geo Geo { get; set; }
    }
}