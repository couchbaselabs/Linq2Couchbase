# Custom JSON Serializers

The Couchbase SDK uses Newtonsoft's Json.Net as its default JSON serializer.  If you are using a custom serializer, there are some special requirements which must be met to support Linq2Couchbase.

Custom serializers are used by creating a class which implements the ITypeSerializer interface, and including it in the SDK configuration.  However, to support Linq2Couchbase, instead the serializer should extend IExtendedTypeSerializer.  This interface provides a key additional feature.

The GetMemberName method is used to determine how a particular member property of a POCO will be written as JSON to the document in Couchbase.  This is important, because the N1QL query must reference member names in the way they appear in Couchbase, not the way they appear in your .Net POCOs.

Here is [an example of how this method was implemented for Newtonsoft's Json.Net](https://github.com/couchbase/couchbase-net-client/blob/03d7957226da6f7c3e05220a21e7ebeeb0519b93/Src/Couchbase/Core/Serialization/DefaultSerializer.cs#L192).

## Non-standard conversions

Some attributes may have additional decorators applied that change how they are serialized. To support this, you should also implement a customer `ISerializationConverterProvider`. This interface can return a custom `ISerializationConverter` for a particular member, altering query generation behavior when this member is used in a N1QL query.

```cs
services.AddCouchbase(options => {
    options.AddLinq(linqOptions => linqOptions.WithSerializationConverterProvider(new MyCustomSerializationConverterProvider()));
});
```

For more details, see [Serialization Converters](./serialization-converters.md).
