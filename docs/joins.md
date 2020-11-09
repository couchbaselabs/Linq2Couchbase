# Joining Documents

Joins are used to combine multiple documents with a common link into a single query result. They work much like traditional joins in SQL, though there are some differences.

> :info: **Note:** Joins can be performed across multiple buckets, so long as the buckets are all on the same cluster.

## Joins and Keys

The fastest form of join is based on document keys. The documents on the left hand side of the join must provide a document key, and it is matched against the document keys on the right hand side of the join.

Joining against document keys is represented in LINQ using N1QlFunctions.Key on the right hand side of the join equality operator.  Examples of this are included in the sections below.

## Inner Joins

An inner join requires that there be a matching document on the right hand side of the join.  If the matching document is not found, the document on the left hand side of the join is dropped from the result set.

```cs
var context = new BucketContext(bucket);

var query = from beer in context.Query<Beer>()
            join brewery in context.Query<Brewery>()
            on beer.BreweryId equals N1QlFunctions.Key(brewery)
            select new {beerName = beer.Name, breweryName = brewery.Name};

await foreach (var doc in query.AsAsyncEnumerable()) {
    // do work
    // will only have documents where the brewery exists
}
```

## Left Outer Joins

A left outer join returns all documents on the left side of the join, even if the document on the right side is not found.

```cs
var context = new BucketContext(bucket);

var query = from beer in context.Query<Beer>()
            join brewery in context.Query<Brewery>()
            on beer.BreweryId equals N1QlFunctions.Key(brewery) into breweryGroup
            from brewery in breweryGroup.DefaultIfEmpty()
            select new {beerName = beer.Name, breweryName = brewery.Name};

await foreach (var doc in query.AsAsyncEnumerable()) {
    // do work
    // will have all beers, even if the brewery document doesn't exist
    // if the brewery document doesn't exist, breweryName will be null
}
```

## Compound Keys

It is also possible to build the key on the left hand side of the join using multiple properties and string constants.  As an example, imagine a bucket with this data structure:

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
    "name":"John Doe"
}
```

As you can see, the key "customer-5" is not present on the order document.  However, you may still join these two documents together using a compound key.

```cs
var context = new BucketContext(bucket);

var query = from order in context.Query<Order>()
            join customer in context.Query<Customer>()
            on "customer-" + order.CustomerId.ToString() equals N1QlFunctions.Key(customer)
            select new {order.OrderId, customer.Name};

await foreach (var doc in query.AsAsyncEnumerable()) {
    // do work
}
```

## ANSI Joins

Beginning with Couchbase Server 5.5, N1QL supports full ANSI joins. It is possible to join against any properties on either side, without using N1QlFunctions.Key. It is necessary, however, to ensure there is an index which can be used to lookup the properties on the right-hand side.

```cs
var context = new BucketContext(bucket);

var query = from route in context.Query<Route>()
            join airport in context.Query<Airport>()
            on route.DestinationAirport equals airport.Faa
            select new {airport.AirportName, route.Airline};

await foreach (var doc in query.AsAsyncEnumerable()) {
{
    // do work
}
```
