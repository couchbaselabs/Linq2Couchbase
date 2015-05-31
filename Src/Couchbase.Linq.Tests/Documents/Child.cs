using Newtonsoft.Json;

namespace Couchbase.Linq.Tests.Documents
{
    public sealed class Child
    {
        [JsonProperty("age")]
        public string Age { get; set; }

        [JsonProperty("fname")]
        public string FirstName { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("contactId")]
        public string ContactId { get; set; }
    }
}