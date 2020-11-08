# Nesting Documents

Nesting documents is somewhat similar to [Joining Documents](joins.md). It has some of the same behaviors and limitations.

However, nesting returns the data in a different format. Nests expect the document on the left to have a list of document keys in an array. Each key on the document is then loaded from the bucket, and a array of documents is provided.  This array may then be manipulated and used elsewhere in the query, such as the select projection or where clause.

> :info: **Note:** Nests can be performed across multiple buckets, so long as the buckets are all on the same cluster.

## Inner Nests

An inner nest requires that there be at least one matching document on the right hand side of the nest.  If a matching document is not found, because none of the documents exist or the array of keys is empty or null, the document on the left hand side of the nest is dropped from the result set.

```cs
var context = new BucketContext(bucket);

var query = context.Query<Brewery>()
            .Nest(
                context.Query<Beer>(),                    // documents to be nested
                brewery => brewery.Beers,                 // array of document keys from left side
                (brewery, beers) => new {brewery, beers}  // select projection for Nest
            );

await foreach (var doc in query.AsAsyncEnumerable()) {
    // do work

    // each returned "document" has a brewery property
    // and a beers property that is an array of beers from that brewery

    // inner nest will not return breweries with no beers
}
```

Note: You may apply other LINQ operations such as where and orderby clauses after the nest operation.

## Left Outer Nests

A left outer nest returns all documents on the left side of the nest, even if there are no documents on the right side is not found.

```cs
var context = new BucketContext(bucket);

var query = context.Query<Brewery>()
            .LeftOuterNest(
                context.Query<Beer>(),                    // documents to be nested
                brewery => brewery.Beers,                 // array of document keys from left side
                (brewery, beers) => new {brewery, beers}  // select projection for Nest
            );

await foreach (var doc in query.AsAsyncEnumerable()) {
    // do work

    // each returned "document" has a brewery property
    // and a beers property that is an array of beers from that brewery

    // may return breweries with no beers
}
```

## Compound Keys

It is also possible to build the key on the left hand side of the nest using multiple properties and string constants.  As an example, imagine a bucket with this data structure:

```text
Key: "order-1001"
{
    "type": "order",
    "orderId": 1001,
    "customerId": 5,
    "amount":123.45
}

Key: "customer-5"
{
    "type": "customer",
    "customerId": 5,
    "name":"John Doe",
    "orders":[
        "1001",
        "1002",
        "1003"
    ]
}
```

As you can see, the key "order-1001" is not present on the customer document.  However, you may still nest the orders using a compound key.

```cs
var context = new BucketContext(bucket);

var query = context.Query<Customer>()
            .Nest(
                context.Query<Order>(),                                // documents to be nested
                customer => customer.Orders.Select(p => "order-" + p), // array of document keys from left side
                (customer, orders) => new {customer, orders}           // select projection for Nest
            );

await foreach (var doc in query.AsAsyncEnumerable()) {
    // do work
}
```

## ANSI Nests

Beginning with Couchbase Server 5.5, N1QL supports full ANSI joins, including ANSI nests. It is now possible to nest against any properties on either side, the N1QlFunctions.Key limitation no longer applies.

To use this feature, simply perform a group join on the desired properties in LINQ. It is necessary, however, to ensure there is an index which can be used to lookup the properties on the right-hand side.

```cs
var context = new BucketContext(bucket);

var query = from airline in context.Query<Airline>()
            join route in context.Query<Route>().Where(route.SourceAirport == "SFO")
                on airline.Iata equals route.Airline into routes
            where airline.Type == "airline" && airline.Country == "United States"
            select new { airline, routes };

await foreach (var doc in query.AsAsyncEnumerable()) {
    // do work

    // each returned "document" has a airline property which is the airline
    // and a routes property that is an array of routes the airline flies from SFO

    // may return airlines with no routes
}
```

To perform an INNER NEST instead of a LEFT OUTER NEST, simply add a .Any() predicate to your where clause.

```cs
var context = new BucketContext(bucket);

var query = from airline in context.Query<Airline>()
            join route in context.Query<Route>().Where(route.SourceAirport == "SFO")
                on airline.Iata equals route.Airline into routes
            where airline.Type == "airline" && airline.Country == "United States" && routes.Any()
            select new { airline, routes };

await foreach (var doc in query.AsAsyncEnumerable()) {
    // do work

    // each returned "document" has a airline property which is the airline
    // and a routes property that is an array of routes the airline flies from SFO

    // will not return airlines with no routes
}
```
