Array Filtering, Projections, and Sorting
=========================================
Using array filtering and projections, you can alter the nature of an array located inside a document before it is returned by N1QL.

## Filtering
To filter an array, simply apply a where clause to the array inside the select projection.  For .Net type consistency, you should apply a ToArray() or ToList() after the subquery.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = from brewery in context.Query<Brewery>()
						select new {
							brewery.Name, 
							longAddresses = (from address in brewery.Address
											 where address.Length > 30
											 select address).ToList()
						};

			foreach (var doc in query) {
				// do work
				// query will return the brewery name and a list of addresses more than 30 chars long
			}
		}
	}

## Projections
Arrays may also be projected to alter the contents of the array.  To perform a projection, simply include it in the select clause of the subquery.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = from brewery in context.Query<Brewery>()
						select new {
							brewery.Name, 
							addresses = (from address in brewery.Address
										 select address.Substring(1)).ToList()
						};

			foreach (var doc in query) {
				// do work
				// query will return the brewery name and a list of the first character of each address line
			}
		}
	}

## Sorting
Arrays may only be sorted by their elements, not by properties or expressions.  If a select projection is performed on the array, then the sort will be on the result of the select projection.  For details on N1QL collation order, see the documentation on the [ORDER BY clause](http://developer.couchbase.com/documentation/server/4.0/n1ql/n1ql-language-reference/orderby.html) in the N1QL documentation.

For LINQ, sorting of an array is represented by the orderby clause, but the argument to the clause must always be the array element.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = from brewery in context.Query<Brewery>()
						select new {
							brewery.Name, 
							addresses = (from address in brewery.Address
										 orderby address).ToList()
						};

			foreach (var doc in query) {
				// do work
				// query will return the brewery name and a sorted list of the addresses
			}
		}
	}

Use the descending keyword to reverse the sort order.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = from brewery in context.Query<Brewery>()
						select new {
							brewery.Name, 
							addresses = (from address in brewery.Address
										 orderby address descending).ToList()
						};

			foreach (var doc in query) {
				// do work
				// query will return the brewery name and a reverse sorted list of the addresses
			}
		}
	}