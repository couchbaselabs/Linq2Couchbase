Date Handling
=============
LINQ works with dates under the assumption that they are stored in the JSON documents as [ISO 8601](https://en.wikipedia.org/wiki/ISO_8601) formatted strings.  For more information about N1QL functions and date handling, see [Date functions](http://developer.couchbase.com/documentation/server/4.0/n1ql/n1ql-language-reference/datefun.html) in the N1QL language reference.

##Date Comparisons
Dates on documents may be compared to each other or to constants using normal .Net comparison operators, so long as the document properties use the DateTime type.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = from beer in context.Query<Beer>()
						where beer.Updated >= new Date(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc)
						select beer;

			foreach (var doc in query) {
				// do work
				// query will return beers updated on or after Jan 1, 2015 0:00 UTC
			}
		}
	}

##Date Functions
At this time, date arithmetic is not supported by this library.  However, support is planned for a future release.

##Time Zones
Note that ISO 8601 date/time fields include time and time zone data.  Be sure to take this into account when working with global systems.