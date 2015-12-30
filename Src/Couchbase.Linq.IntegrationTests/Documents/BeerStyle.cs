using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Couchbase.Linq.IntegrationTests.Documents
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BeerStyle
    {
        Porter,

        [EnumMember(Value="Oatmeal Stout")]
        OatmealStout,

        [EnumMember(Value="Pumpkin Beer")]
        PumpkinBeer
    }
}
