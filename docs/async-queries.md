Asynchronous Queries
====================
For performance in high load web environments, Linq2Couchbase supports executing queries asynchronously.  This helps optimize thread utilization by avoiding thread blocking during long running query operations.

## Executing Basic Queries
To execute basic queries asynchronously, simply add .ExecuteAsync() to the end of the query building lambda chain.

```
var result = await context.Query<Beer>().Where(p => p.Abv >= 6).ExecuteAsync();

// or

var result = await (from p in context.Query<Beer>()
                    where p.Abv >= 6
                    select p).ExecuteAsync();
```

**Note:** ExecuteAsync will be your query execution immediately.  It does not wait for you to enumerate the collection before executing.

**Note:** You also cannot apply any more expressions to the query after calling ExecuteAsync.  If you apply additional expressions after the ExecuteAsync call, they will be applied in-memory rather than in the N1QL query, resulting in reduced performance.

**Note:** ExecuteAsync will only work for Linq2Couchbase queries.  Code completion will show it as an option for other types of LINQ queries, but it will fail when executed.

## Executing Scalar Queries
Scalar queries are queries that return a single result, instead of a list of results.  Queries ending in First, Single, and Sum are examples of scalar queries.

The last command in a scalar query typically executes the query, therefore once called it's already too late to call ExecuteAsync to make the query run asynchronously.  To avoid this, pass the final expression to ExecuteAsync as a lambda expression.

```
// Example synchronous query
var result = context.Query<Beer>().Where(p => p.Abv == 6).First();

// Example asynchronous equivalent
var result = await context.Query<Beer>().Where(p => p.Abv == 6).ExecuteAsync(query => query.First());

// Example synchronous query
var result = context.Query<Beer>().Average(p => p.Abv);

// Example asynchronous equivalent
var result = await context.Query<Beer>().ExecuteAsync(query => query.Average(p => p.Abv));
```
