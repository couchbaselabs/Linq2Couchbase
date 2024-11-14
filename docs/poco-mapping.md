# Mapping JSON fields to POCO properties

> [NOTE]
> The documetation has been updated to reflect that the product name for N1QL has been changed to SQL++, however, the source itself may still use the name N1QL.

While not an actually responsibility of the LINQ provider, the default serialization library, Newtonsoft.Json, supports field the following serialization attributes:

- [DataContractAttribute](https://msdn.microsoft.com/en-us/library/ms585243)
- [DataMemberAttribute](https://msdn.microsoft.com/en-us/library/ms574795)
- [NonSerializedAttribute](msdn2.microsoft.com/en-us/library/z951x24h)
- All Newtonsoft.Json [attributes](http://www.newtonsoft.com/json/help/html/serializationattributes.htm).

Please refer to their documentation for issues regarding serialization.

Mixing of attribute types is supported by Newtonsoft.Json. Here is an example of mixing JsonPropertyAttribute and DataMemberAttribute:

```cs
public class Beer
{
    [Key]
    [JsonProperty("name")]
    public string Name { get; set; }

    [DataMember(Name = "abv")]
    public decimal Abv { get; set; }

    [JsonProperty("ibu")]
    public decimal Ibu { get; set; }

    // ...
}
```

See [Custom JSON Serializers](./custom-serializers.md) for information on using a custom JSON serializer.
