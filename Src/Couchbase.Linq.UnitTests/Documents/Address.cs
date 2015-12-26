using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Couchbase.Linq.UnitTests.Documents
{
    class Address
    {
        [JsonProperty("address1")]
        public string AddressLine1 { get; set; }
    }
}
