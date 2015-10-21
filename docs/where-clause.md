Filtering with Where
====================
The where clause is used to apply filter predicates to your query.  It supports a very wide variety of operators and methods when translating to N1QL.

Basic Usage
===========
To apply a where clause, simply add it to your LINQ query along with an expression that returns a Boolean result.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample") {
			var context = new BucketContext(bucket);

			var query = from beer in context.Query<Beer>()
						where beer.Abv == 6
						select beer;

			foreach (var doc in query) {
				// do work
			}
		}
	}

The above example will return all beers with an ABV of exactly 6.

Compound Expressions
====================
The where clause supports Boolean operations (&& and ||) to combine multiple predicates.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample") {
			var context = new BucketContext(bucket);

			var query = from beer in context.Query<Beer>()
						where (beer.Abv == 6) && (beer.Name != null)
						select beer;

			foreach (var doc in query) {
				// do work
			}
		}
	}

It is also valid to extend the query with multiple where clauses.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample") {
			var context = new BucketContext(bucket);

			var query = from beer in context.Query<Beer>()
						where.beer.Abv == 6
						select beer;

			if (nameRequired) { // local variable
				query = from beer in query
						where beer.Name != null
						select beer;
			}

			foreach (var doc in query) {
				// do work
			}
		}
	}

Document Type Filters
=====================
Some where clauses may be automatically applied to your query based on the document type you are querying.  For example, it is common to use `DocumentTypeFilter` attributes on your document objects so they are automatically limited to documents with a certain "type" attribute.  For more information, see [Mapping JSON documents to POCOs with DocumentFilters](https://github.com/couchbaselabs/Linq2Couchbase/blob/master/docs/document-filters.md).

Supported Operators
===================
The following operators are supported by Linq2Couchbase.

- Equality (==)
- Inequality (!=)
- Greater Than (>)
- Less Than (<)
- Greater Than Or Equal (>=)
- Less Than Or Equal (<=)
- Boolean And (&&)
- Boolean Or (||)
- Boolean Not (!)
- Addition (+)
- Subtraction/Negation (-)
- Multiplication (*)
- Division (/)
- Modulus (%)
- Coalesce (??)
- Conditional Expressions (a ? b : c)