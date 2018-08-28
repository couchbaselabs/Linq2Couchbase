Grouping and Aggregation
========================
LINQ may also be used to group query results and perform aggregation of data on the groupings.  This grouping and aggregation is performed on the query server for improved performance.

**Note:** LINQ group joins are not currently supported.

## Simple Group
Grouping support is provided by the LINQ group clause.  When grouping, you provide a key selector and an into clause.  The key selector defines which property should be used to group.

The into clause provides a local name for the group to be referenced by for the rest of the query.  When referencing this name, it is a list of the documents in a given group.  It also has a .Key property, which is the value of the key selector for that group.

For more information about how the group clause works, see the [MSDN group clause reference](https://msdn.microsoft.com/en-us/library/bb384063.aspx).

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = from beer in context.Query<Beer>()
                		group beer by beer.BreweryId
                		into g
                		select g.Key;

			foreach (var doc in query) {
				// do work
				// query will return the list of distinct brewery ids from all beers
				// Note: This form is shown for simplicity, using .Distinct() is probably a better solution
			}
		}
	}

It is also possible to group by a combination of multiple columns.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = from beer in context.Query<Beer>()
                		group beer by new {beer.BreweryId, beer.Category}
                		into g
                		select new {g.Key.BreweryId, g.Key.Category};

			foreach (var doc in query) {
				// do work
				// query will return the list of distinct brewery ids and categories from all beers
				// Note: This form is shown for simplicity, using .Distinct() is probably a better solution
			}
		}
	}

## Applying An Aggregate To A Group
LINQ supports six aggregates which can be applied to a group.

- Sum - Adds values
- Average - Averages values
- Min - Returns the lowest value
- Max - Returns the highest value
- Count - Returns the number of records
- LongCount - Returns the number of records as a 64-bit integer

Sum, Average, Min, and Max all accept a selector parameter that defines which property should be operated on.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = from beer in context.Query<Beer>()
                		group beer by beer.BreweryId
                		into g
                		select new {breweryId = g.Key, avgAbv = g.Average(p => p.Abv)};

			foreach (var doc in query) {
				// do work
				// query will return the list of brewery ids and the average ABV from that brewery
			}
		}
	}

Count and LongCount do not require a parameter.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = from beer in context.Query<Beer>()
                		group beer by beer.BreweryId
                		into g
                		select new {breweryId = g.Key, numBeers = g.Count()};

			foreach (var doc in query) {
				// do work
				// query will return the list of brewery ids and the number of beers produced by that brewery
			}
		}
	}

## Ungrouped Aggregates
It is not required to use grouping to use an aggregate function.  It is also possible to apply the aggregate directly to a query.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var result = (from beer in context.Query<Beer>()
                		  select beer).LongCount();

			// result is the total number of beers in the bucket
		}
	}

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var result = (from beer in context.Query<Beer>()
						  where beer.BreweryId = "21st_amendment_brewery_cafe"
                		  select beer).Average(p => p.Abv);

			// result is the average ABV of all beers made by 21st Amendment Brewery Cafe
		}
	}

## Distinct Counts
Count aggregates also support the .Distinct() function.  They return the number of discount results, rather than the total number of results.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var result = (from beer in context.Query<Beer>()
                		  select beer.Name).Distinct().LongCount();

			// result is the total number of distinct beer names
		}
	}