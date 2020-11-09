# Date Handling

LINQ works with dates under the assumption that they are stored in the JSON documents as [ISO 8601](https://en.wikipedia.org/wiki/ISO_8601) formatted strings. For more information about N1QL functions and date handling, see [Date functions](http://developer.couchbase.com/documentation/server/4.0/n1ql/n1ql-language-reference/datefun.html) in the N1QL language reference.

Date/times stored as milliseconds since the Unix epoch are also supported. If you are using the default Newtonsoft.Json serializer, simply add the `[JsonConverter(typeof(UnixMillisecondsConverter)]` attribute to the property. For custom serializers, see [Custom JSON Serializers](./custom-serializers.md).

## Date Comparisons

Dates on documents may be compared to each other or to constants using normal .Net comparison operators, so long as the document properties use the DateTime type.

```cs
using (var cluster = new Cluster()) {
    using (var bucket = cluster.OpenBucket("beer-sample")) {
        var context = new BucketContext(bucket);

        var query = from beer in context.Query<Beer>()
                    where beer.Updated >= new Date(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    select beer;

        foreach (var doc in query) {
            // do work
            // query will return beers updated on or after Jan 1, 2015 0:00 UTC
        }
    }
}
```

## Date Functions

A subset of N1QL date/time functions are supported for use in LINQ queries, and are provided as static methods of the N1QlFunctions class.

```cs
using (var cluster = new Cluster()) {
    using (var bucket = cluster.OpenBucket("beer-sample")) {
        var context = new BucketContext(bucket);

        var query = from beer in context.Query<Beer>()
                    where N1QlFunctions.DateDiff(DateTime.Now, beer.Updated, N1QlDatePart.Day) > 10
                    select beer;

        foreach (var doc in query) {
            // do work
            // query will return beers last updated more than 10 days ago
        }
    }
}
```

| Function Name               | N1QL Equivalent |
| --------------------------- | --------------- |
| N1QlFunctions.DateDiff      | DATE_DIFF_STR   |
| N1QlFunctions.DateAdd       | DATE_ADD_STR    |
| N1QlFunctions.DatePart      | DATE_PART_STR   |
| N1QlFunctions.DateTrunc     | DATE_TRUNC_STR  |

These methods may only be used within queries.  They cannot be called directly in code.  This also means that they cannot be used in unit tests where the query source is being faked.  This may be improved in a future version.

## Time Zones

Note that ISO 8601 date/time fields include time and time zone data.  Be sure to take this into account when working with global systems.
