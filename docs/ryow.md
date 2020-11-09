# Using Read Your Own Write (RYOW)

When writing changes (mutating documents), Couchbase Server indexes are not updated immediately. This allows improved write performance, because it isn't necessary for mutations to wait for index updates to complete before returning. This index update delay doesn't affect CRUD operations via the SDK, but it does affect the results returned by N1QL queries.

This delay is often unnoticed. However, there can be circumstances where you need to execute a query immediately following a mutation, and need the mutation to be included in the query result. For example, if you are posting a new sales order, and then query the total sales for that customer after posting the new order.

To enable Read Your Own Write when executing query, you specifically indicate which updates must be processed by the indexer before returning query results.  This may add a delay to your query results, but you are guaranteed that your mutations are included in the query result.  It is also more performant than AtPlus consistency, because you only need to wait for your mutations to be indexed, not all mutations.  More information is available [here](http://developer.couchbase.com/documentation/server/current/developer-guide/query-consistency.html).

## Reading Your Own Writes

Each `BucketContext` will automatically keep track of its own `MutationState` as mutations are applied.  In order to execute a query using this state, simply use the `ConsistentWith` method when building your query.

```cs
using Couchbase.Linq.Extensions;

// ...

var insertResult = await bucket.DefaultCollection().InsertAsync("doc-key", docValue));

var context = new BucketContext(bucket);

var query = context.Query<Beer>()
    .ConsistentWith(MutationState.From(insertResult))
    .Count();
```

> :info: **Note:** The `ConsistentWith` method should only be invoked on the main extent being queried. If you are performing joins or nests, don't include ConsistentWith on these extents.
