﻿using Newtonsoft.Json;

namespace Couchbase.Linq.Tests.Documents
{
    public class Geo
    {
        [JsonProperty("accuracy")]
        public string Accuracy { get; set; }

        [JsonProperty("lat")]
        public string Latitude { get; set; }

        [JsonProperty("lon")]
        public string Longitude { get; set; }
    }
}