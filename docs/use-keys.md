The UseKeys Method
==================
The UseKeys method is used to select documents based on a list of zero or more keys.  In this way, it is similar to performing a multiple get using `IBucket.Get<T>(IList<string> keys)`.  The UseKeys method is also required when performing subqueries.

**Note:** You must import the `Couchbase.Linq.Extensions` namespace to use the UseKeys method.

##Basic Usage##
The call to UseKeys should be immediately after the call to Query<T>.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample") {
			var context = new BucketContext(bucket);

			var keys = new[] {"alesmith_brewing-wee_heavy", "ali_i_brewing-amber_ale"}; 

			var query = from beer in context.Query<Beer>().UseKeys(keys)
						select beer;
		}
	}

The query in this example will return all Beer documents which match the list of keys.

The advantage of this approach over using `IBucket.Get<T>` is that you may then expand upon the query by applying where predicates, sorting, select projections, joins to other documents, etc.  However, for simple queries like the example above, using `IBucket.Get<T>` will be more performant because it will work directly against the data nodes instead of passing through the query and index nodes.

##Subqueries##
When performing subqueries, the primary document type being queried must include the UseKeys method.  In a subquery, however, you have the added flexibility to use values from the main query to provide the list of keys.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample") {
			var context = new BucketContext(bucket);

			var query = from brewery in context.Query<Brewery>()
						select new {
							name = brewery.Name,
							highAbvBeers = 
								(from beer in context.Query<Beer>().UseKeys(brewery.Beers)
								 where beer.Abv >= 6
								 select beer).ToList()
						};
		}
	}

The query example returns a list of breweries, with a nested list of beers with an ABV greater than or equal to 6.

Note that this example assumes an attribute "beers" on brewery documents, which is an array of string keys for all beers made by that brewery.  The actual beer-sample bucket doesn't contain this document attribute.  If you want to test this sample against beer-sample, you should modify one of the brewery documents to include this attribute.