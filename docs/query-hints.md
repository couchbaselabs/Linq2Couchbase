# Query Hints

> [NOTE]
> The documetation has been updated to reflect that the product name for N1QL has been changed to SQL++, however, the source itself may still use the name N1QL.

## UseIndex

The UseIndex method is used to provide an index hint to the query engine.  This can help improve query performance in cases where Explain shows that the index being used by default is inefficient.

> :info: **Note:** You must import the `Couchbase.Linq.Extensions` namespace to use the UseIndex method. Also, you may only call UseIndex on the first, primary keyspace being queried. It cannot be used after join clauses.

## Basic Usage

The call to `UseIndex` should be immediately after the call to `Query<T>`.

```cs
var context = new BucketContext(bucket);

var query = from beer in context.Query<Beer>().UseIndex("beer_abv")
            where beer.Abv > 6
            select beer;
```

The query in this example will return all Beer documents which have an ABV greater than 6.  It will use an index named "beer_abv" to optimize the query, if it exists.

## Index Types

By default, UseIndex assumes you are using a GSI index.  However, it is also possible to query a View index.  UseIndex accepts an optional second parameter indicating the index type.

```cs
var context = new BucketContext(bucket);

var query = from beer in context.Query<Beer>().UseIndex("beer_abv", N1QlIndexType.View)
            where beer.Abv > 6
            select beer;
```

Note that views must be defined using a `CREATE INDEX` statement in order to be usable via SQL++ queries.

## Hash Joins

Couchbase Server 5.5 Enteprise Edition also support hash joins.  This is useful for large joins, where it is more efficient than the default nested loop join.  This type of join is not supported by Community Edition.

To provide a hash join hint, add it to the right hand extent of the join.  You may choose to perform a Build or Probe hash join.  For details, see [Example 9 of this blog post](https://blog.couchbase.com/ansi-join-support-n1ql/).

```cs
var context = new BucketContext(bucket);

var query = from route in context.Query<Route>()
            join airport in context.Query<Airport>().UseHash(HashHintType.Build)
                on route.DestinationAirport equals airport.Faa
            select new {route, airport};
```
