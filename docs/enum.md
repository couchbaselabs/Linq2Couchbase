Working With Enumerations
=========================

For the most part, enumeration properties are supported transparently.  LINQ queries can filter based on enumeration values just like other properties:

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = from beer in context.Query<Beer>()
						where beer.Style == BeerStyle.Porter
						select beer;

			foreach (var doc in query) {
				// do work
			}
		}
	}

	// Note: this example assumes the Beer document is configured with Style as enumeration property

So long as you are using the standard JSON serializer, which serializes the enumeration value as a number, there are no special concerns to be addressed.

## Non-Standard JSON Converters##

If a non-standard JSON converter is being used, there are some additional limitations.  A common example of this would be the [Json.Net StringEnumConverter](http://www.newtonsoft.com/json/help/html/t_newtonsoft_json_converters_stringenumconverter.htm).  This converter serializes the enumeration as a string, rather than as an integer.

In this case, there are some additional rules that must be followed.

1. Only equals (==) and not equals (!=) comparisons are supported.  Greater than, less than, etc are not supported.
2. The custom serializer must be directly applied to the enumeration itself, not to the document or the property.

For example, this is supported:

	[JsonConverter(typeof(StringEnumConverter))]
	public enum MyEnum
	{
		[EnumMember(Value="Value 1")]
		Value1,

		[EnumMember(Value="Value 2")]
		Value2
	}

However, this approach is not:

	public class MyDocument
	{
		// This is not supported, because we are changing how this property is serialized,
	    // rather than changing how the enumeration is serialized
		[JsonConverter(typeof(StringEnumConverter))]
		public MyEnum EnumValue { get; set; }
	}