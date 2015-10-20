using System.Collections.Generic;
using Newtonsoft.Json;

namespace Couchbase.Linq.UnitTests.Documents
{
    public sealed class Contact
    {
        [JsonProperty("age")]
        public int Age { get; set; }

        [JsonProperty("fname")]
        public string FirstName { get; set; }

        [JsonProperty("lname")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("hobbies")]
        public List<string> Hobbies { get; set; }

        [JsonProperty("relation")]
        public string Relation { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("children")]
        public List<Child> Children { get; set; }
    }


    /*
     *    {
      "age": 46,
      "children": [
        {
          "age": 17,
          "fname": "Aiden",
          "gender": "m"
        },
        {
          "age": 2,
          "fname": "Bill",
          "gender": "f"
        }
      ],
      "email": "dave@gmail.com",
      "fname": "Dave",
      "hobbies": [
        "golf",
        "surfing"
      ],
      "lname": "Smith",
      "relation": "friend",
      "title": "Mr.",
      "type": "contact"
    },*/
}