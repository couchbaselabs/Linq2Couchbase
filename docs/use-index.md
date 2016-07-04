The UseIndex Method
==================
The UseIndex method is used to provide an index hint to the query engine.  This can help improve query performance in cases where Explain shows that the index being used by default is inefficient.

**Note:** You must import the `Couchbase.Linq.Extensions` namespace to use the UseIndex method.

**Note:** You may only call UseIndex on the first, primary keyspace being queried.  It cannot be used after join clauses.

##Basic Usage
The call to UseIndex should be immediately after the call to Query<T>.

	var context = new BucketContext(bucket);

	var query = from beer in context.Query<Beer>().UseIndex("beer_abv")
				where beer.Abv > 6
				select beer;

The query in this example will return all Beer documents which have an ABV greater than 6.  It will use an index named "beer_abv" to optimize the query, if it exists.

##Index Types
By default, UseIndex assumes you are using a GSI index.  However, it is also possible to query a View index.  UseIndex accepts an optional second parameter indicating the index type.

	var context = new BucketContext(bucket);

	var query = from beer in context.Query<Beer>().UseIndex("beer_abv", N1QlIndexType.View)
				where beer.Abv > 6
				select beer;

Note that views must be defined using a `CREATE INDEX` statement in order to be usable via N1QL queries.