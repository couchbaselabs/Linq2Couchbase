# NESTing Documents

Nesting documents is somewhat similar to [JOINing Documents](joins.md).  It has some of the same behaviors and limitations.

However, nesting returns the data in a different format.  Nests expect the document on the left to have a list of document keys in an array.  Each key on the document is then loaded from the bucket, and a array of documents is provided.  This array may then be manipulated and used elsewhere in the query, such as the select projection or where clause.

**Note:** Nests can be performed across multiple buckets, so long as the buckets are all on the same cluster.

## Nests and Keys

In N1QL, all nest operations must be done using document keys.  The documents on the left hand side of the nest must provide an array of document keys, and these keys are matched against the document keys on the right hand side of the join.  You may not nest against document properties on the right hand side of the nest, so it is important to take this into consideration when designing your data model.

Note: The examples below use the beer-sample bucket, and assume that brewery documents have an array of keys of beer documents.  This isn't actually the case in the default beer-sample bucket.  If you want to run these examples, you'll need to modify the brewery documents to support it.

## Inner Nests

An inner nest requires that there be at least one matching document on the right hand side of the nest.  If a matching document is not found, because none of the documents exist or the array of keys is empty or null, the document on the left hand side of the nest is dropped from the result set.

```cs
using (var cluster = new Cluster()) {
    using (var bucket = cluster.OpenBucket("beer-sample")) {
        var context = new BucketContext(bucket);

        var query = context.Query<Brewery>()
                    .Nest(
                        context.Query<Beer>(),                    // documents to be nested
                        brewery => brewery.Beers,                 // array of document keys from left side
                        (brewery, beers) => new {brewery, beers}  // select projection for Nest
                    );

        foreach (var doc in query) {
            // do work

            // each returned "document" has a brewery property
            // and a beers property that is an array of beers from that brewery

            // inner nest will not return breweries with no beers
        }
    }
}
```

Note: You may apply other LINQ operations such as where and orderby clauses after the nest operation.

## Left Outer Nests

A left outer nest returns all documents on the left side of the nest, even if there are no documents on the right side is not found.

```cs
using (var cluster = new Cluster()) {
    using (var bucket = cluster.OpenBucket("beer-sample")) {
        var context = new BucketContext(bucket);

        var query = context.Query<Brewery>()
                    .LeftOuterNest(
                        context.Query<Beer>(),                    // documents to be nested
                        brewery => brewery.Beers,                 // array of document keys from left side
                        (brewery, beers) => new {brewery, beers}  // select projection for Nest
                    );

        foreach (var doc in query) {
            // do work

            // each returned "document" has a brewery property
            // and a beers property that is an array of beers from that brewery

            // may return breweries with no beers
        }
    }
}
```

## Compound Keys

It is also possible to build the key on the left hand side of the nest using multiple properties and string constants.  As an example, imagine a bucket with this data structure:

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

As you can see, the key "order-1001" is not present on the customer document.  However, you may still nest the orders using a compound key.

```cs
using (var cluster = new Cluster()) {
    using (var bucket = cluster.OpenBucket("beer-sample")) {
        var context = new BucketContext(bucket);

        var query = context.Query<Customer>()
                    .Nest(
                        context.Query<Order>(),                                // documents to be nested
                        customer => customer.Orders.Select(p => "order-" + p), // array of document keys from left side
                        (customer, orders) => new {customer, orders}           // select projection for Nest
                    );

        foreach (var doc in query) {
            // do work
        }
    }
}
```

## Index Nests

Beginning with Couchbase Server 4.5, it is possible to perform nests where the keys are stored in the child document.  Previously, the parent document needed a list of keys for the child documents being nested.

This kind of nest operation is actually more consistent with LINQ standards, and is represented by the group join construct.  The requirement is that the left hand side of the join equality operator must use N1QlFunctions.Key to get the key from one of the other extents in the query.

```cs
using (var cluster = new Cluster()) {
    using (var bucket = cluster.OpenBucket("beer-sample")) {
        var context = new BucketContext(bucket);

        var query = from brewery in context.Query<Brewery>()
            join beer in context.Query<Beer>()
                on N1QlFunctions.Key(brewery) equals beer.BreweryId into beers
            select new {brewery, beers};

        foreach (var doc in query) {
            // do work

            // each returned "document" has a brewery property
            // and a beers property that is an array of beers from that brewery

            // may return breweries with no beers
        }
    }
}
```

In order to perform this type of join, there must also be an index created on the key field in the child document.  In the example above, the index must be on the BreweryId field.  More information about this type of join operation can be found [here](http://developer.couchbase.com/documentation/server/4.5-dp/flexible-join-n1ql.html).  Note that, unlike index joins, for index nest operations the index **must not** have a WHERE clause when it is created.

Note that a NotSupportedException will be thrown if you execute this kind of nest operation against a 4.0 or 4.1 Couchbase cluster.

## ANSI Nests

Beginning with Couchbase Server 5.5, N1QL supports full ANSI joins, including ANSI nests. It is now possible to nest against any properties on either side, the N1QlFunctions.Key limitation no longer applies.

To use this feature, simply perform a group join on the desired properties in LINQ. It is necessary, however, to ensure there is an index which can be used to lookup the properties on the right-hand side. Attempting to use this feature on a Couchbase Server cluster before version 5.5 will result in a NotSupportedException.

```cs
var context = new BucketContext(bucket);

var query = from airline in context.Query<Airline>()
            join route in context.Query<Route>().Where(route.SourceAirport == "SFO")
                on airline.Iata equals route.Airline into routes
            where airline.Type == "airline" && airline.Country == "United States"
            select new { airline, routes };

foreach (var doc in query) {
    // do work

    // each returned "document" has a airline property which is the airline
    // and a routes property that is an array of routes the airline flies from SFO

    // may return airlines with no routes
}
```