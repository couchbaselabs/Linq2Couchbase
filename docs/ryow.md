Using Read Your Own Write (RYOW)
================================
When writing changes (mutating documents), Couchbase Server indexes are not updated immediately.  This allows improved write performance, because it isn't necessary for mutations to wait for index updates to complete before returning.  This index update delay doesn't affect CRUD operations via the SDK, but it does affect the results returned by N1QL queries.

This delay is often unnoticed.  However, there can be circumstances where you need to execute a query immediately following a mutation, and need the mutation to be included in the query result.  For example, if you are posting a new sales order, and then query the total sales for that customer after posting the new order.

Couchbase Server 4.5 adds support for this use case using Read Your Own Write consistency.  To use this feature, you specifically indicate which updates must be processed by the indexer before returning query results.  This may add a delay to your query results, but you are guaranteed that your mutations are included in the query result.  It is also more performant than AtPlus consistency, because you only need to wait for your mutations to be indexed, not all mutations.  More information is available [here](http://developer.couchbase.com/documentation/server/current/developer-guide/query-consistency.html).

## Configuring The Client
To support Read Your Own Write, your Couchbase bucket must be configured for enhanced durability.  If not, no exceptions will be thrown but consistency is not guaranteed.

Example XML configuration section:

```xml
<couchbaseClients>
  <couchbase>
    <servers>
      <add uri="http://localhost:8091"></add>
    </servers>
    <buckets>
      <add name="beer-sample" useEnhancedDurability="true"></add>
    </buckets>
  </couchbase>
</couchbaseClients>
```

## Reading Your Own Writes
Each `BucketContext` will automatically keep track of its own `MutationState` as mutations are applied.  In order to execute a query using this state, simply use the `ConsistentWith` method when building your query.

```csharp
using Couchbase.Linq.Extensions;

// ...

var context = new BucketContext(bucket);
context.Save(someDocument);

var query = context.Query<Beer>()
	.ConsistentWith(context.MutationState)
	.Count();
```

**Note:** The `ConsistentWith` method should only be invoked on the main extent being queried.  If you are performing joins or nests, don't include ConsistentWith on these extents.

##Combining Multiple MutationStates
If you are working with multiple BucketContexts, it is possible to use MutationState values from both buckets to ensure that mutations on each are included in your query.

```csharp
using Couchbase.Linq.Extensions;

// ...

var context = new BucketContext(bucket);
context.Save(someDocument);

var savedState = context.MutationState;

// ...

var context2 = new BucketContext(bucket);
context2.Save(someOtherDocument);

var query = context2.Query<Beer>()
	.ConsistentWith(savedState)
	.ConsistentWith(context2.MutationState)
	.Count();
```

## Working With Change Tracking
When using change tracking, the `MutationState` value won't be meaningful until after you call `SubmitChanges`.

```csharp
using Couchbase.Linq.Extensions;

// ...

var context = new BucketContext(bucket);

context.BeginChangeTracking();
context.Save(someDocument);

// This query will NOT include the updates to someDocument
var query = context.Query<Beer>()
	.ConsistentWith(context.MutationState)
	.Count();

context.SubmitChanges();

// This query will include the updates to someDocument
var query2 = context.Query<Beer>()
	.ConsistentWith(context.MutationState)
	.Count();
```

## Performance Note
To support Read Your Own Write consistency, each `BucketContext` builds the `MutationState` on an ongoing basis as each mutation is completed.  Normally, the `BucketContext` is a short-lived object that is created and thrown away quickly.  However, if you are using long-lived `BucketContext` objects, memory utilization may tend to increase over time as more mutations are added.  In this case, it is recommended to run `BucketContext.ResetMutationState` regularly to clear the `MutationState` and reduce memory utilization.