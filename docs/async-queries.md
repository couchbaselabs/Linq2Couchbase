# Asynchronous Queries

For performance in high load web environments, Linq2Couchbase supports executing queries asynchronously. This helps optimize thread utilization by avoiding thread blocking during long running query operations.

> :info: **Note:** The methods below will only work for Linq2Couchbase queries. Code completion will show them as an option for other types of LINQ queries, but they will fail when executed. Other libraries, such as Entity Framework Core, have their own versions of these methods designed for use with their queries.

## Asynchronous Enumeration

To execute basic queries asynchronously, simply add .AsAsyncEnumerable() to the end of the query building lambda chain. This approach has the advantage that the individual query results are processed as they arrive from the query node. In some cases this may increase throughput and reduce memory utilization.

```cs
var query = await context.Query<Beer>().Where(p => p.Abv >= 6).AsAsyncEnumerable();

// or

var query = await (from p in context.Query<Beer>()
                    where p.Abv >= 6
                    select p).AsAsyncEnumerable();
```

The query may then been executed and the results processed using an `await foreach` loop.

```cs
await foreach (var item in query.WithCancellation(cancellationToken))
{
    // Do work
}
```

## Simple Asynchronous Query To A List

Using IAsyncEnumerable can be more cumbersome when used with C# versions less than 8. Also, sometimes you know you need a list of all results. In that case, `ToListAsync()` may be used to execute the query asynchronously but return a list of all results.

```cs
var results = await context.Query<Beer>().Where(p => p.Abv >= 6).ToListAsync(cancellationToken);
```

## Executing Scalar Queries

Scalar queries are queries that return a single result, instead of a list of results. Linq2Couchbase offers asynchronous overloads of First, Single, Sum, Average, Min, Max, Count, LongCount, Any, All, and Explain.

```cs
var result = context.Query<Beer>().Where(p => p.Abv == 6).FirstAsync(cancellationToken);
```
