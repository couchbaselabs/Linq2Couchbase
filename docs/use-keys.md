# The UseKeys Method

The UseKeys method is used to select documents based on a list of zero or more keys.  In this way, it is similar to performing a multiple get using `IBucket.Get<T>(IList<string> keys)`.  The UseKeys method is also required when performing subqueries.

**Note:** You must import the `Couchbase.Linq.Extensions` namespace to use the UseKeys method.

## Basic Usage

The call to UseKeys should be immediately after the call to `Query<T>`.

```cs
var context = new BucketContext(bucket);

var keys = new[] {"alesmith_brewing-wee_heavy", "ali_i_brewing-amber_ale"};

var query = from beer in context.Query<Beer>().UseKeys(keys)
            select beer;
```

The query in this example will return all Beer documents which match the list of keys.

The advantage of this approach over using `ICouchbaseCollection.GetAsync<T>` is that you may then expand upon the query by applying where predicates, sorting, select projections, joins to other documents, etc.  However, for simple queries like the example above, using `ICouchbaseCollection.GetAsync<T>` will be more performant because it will work directly against the data nodes instead of passing through a query node.
