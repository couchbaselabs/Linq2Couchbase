Testing For NULL And MISSING Attributes
=======================================
Like SQL, N1QL allows you to test for NULL values in your queries.  Additionally, N1QL adds the concept of MISSING, which means that the attribute isn't present in the document.  NULL means that the attribute is present in the document, but set specifically to null.

## Testing For NULL
Testing for nulls works just like you would expect in LINQ.  You simply use == or != to compare to null.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = from brewery in context.Query<Brewery>()
						where brewery.Geo != null
						select brewery;

			foreach (var doc in query) {
				// do work
			}
		}
	}

For Nullable value types (i.e. "integer?" or "DateTime?") you may also test for nulls using the HasValue property.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = from beer in context.Query<Beer>()
						where beer.SomeNullableProperty.HasValue
						select beer;

			foreach (var doc in query) {
				// do work
			}
		}
	}

## Testing For MISSING
Testing for missing attributes is done using N1QlFunctions.IsMissing and N1QlFunctions.IsNotMissing.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = from beer in context.Query<Beer>()
						where N1QlFunctions.IsNotMissing(beer.Abv)
						select beer;

			foreach (var doc in query) {
				// do work
			}
		}
	}

This query will return all beers where there is an "abv" attribute present on the document, even if the value is null.  Only documents where the attribute is missing will be filtered out of the results.

## Testing For Valued Attributes
Testing for null and missing are very specific tests.  If you test for null, it returns false if the value is missing.  If you test for missing, it returns false if the value is null.  N1QL also provides the concept of an attribute being valued.  This combines the two concepts, considering both nulls and missing values to be not valued.  This is implemented in LINQ using N1QlFunctions.IsValued and N1QlFunctions.IsNotValued. 

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = from beer in context.Query<Beer>()
						where N1QlFunctions.IsNotValued(beer.Abv)
						select beer;

			foreach (var doc in query) {
				// do work
			}
		}
	}

This query returns all beers where Abv is either null or missing.

## Coalescing
Coalescing is also supported, using the c# "??" operator.  For VB.Net, coalesce is the "If(a, b)" operation.  LINQ will return the second argument if the first argument is missing or null.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = from beer in context.Query<Beer>()
						select new {name = beer.Name, abv = beer.Abv ?? 0};

			foreach (var doc in query) {
				// do work
			}
		}
	}

This query will return 0 for any beer that has a null or missing Abv.

## Unit Testing Limitations
Internally, .Net doesn't have an equivalent for the concept of missing.  Therefore, when mocking Couchbase in your unit tests N1QlFunctions.IsMissing always return false, and N1QlFunctions.IsNotMissing always returns true.  N1QlFunctions.IsValued returns the same result as "!= null", and N1QlFunctions.IsNotValued returns the same result as "== null".