Mapping JSON fields to POCO properties
======================================
While not an actually responsibility of the Linq provider, the default serialization library, NewtonSoft, supports field the following serialization attributes:

- [DataContractAttribute](https://msdn.microsoft.com/en-us/library/ms585243)
- [DataMemberAttribute](https://msdn.microsoft.com/en-us/library/ms574795)
- [NonSerializedAttribute](msdn2.microsoft.com/en-us/library/z951x24h)
- All NewtonSoft [attributes](http://www.newtonsoft.com/json/help/html/serializationattributes.htm).

Please refer to their documentation for issues regarding serialization. Note that in a future release, custom [ITypeSerializer](http://blog.couchbase.com/2015/june/using-jil-for-custom-json-serialization-in-the-couchbase-.net-sdk) implementations for serializers other than NewtonSoft will be supported, in which case the attributes supported by any other 3rd party serializers implemented will need to be used. 

Mixing of attribute types is supported by NewtonSoft. Here is an example of mixing JsonPropertyAttribute and DataMemberAttribute:

	 public class Beer
     {
        [Key]
        [JsonProperty("name")]
        public string Name { get; set; }

        [DataMember(Name = "abv")]
        public decimal Abv { get; set; }

        [JsonProperty("ibu")]
        public decimal Ibu { get; set; }

        ...
     }

If you think you maybe be using a custom serializer when support becomes available, you may want to use the .NET attributes.