NESTing Documents
=================
Nesting documents is somewhat similar to [JOINing Documents](https://github.com/couchbaselabs/Linq2Couchbase/blob/master/docs/joins.md).  It has some of the same behaviors and limitations.

However, nesting returns the data in a different format.  Nests expect the document on the left to have a list of document keys in an array.  Each key on the document is then loaded from the bucket, and a array of documents is provided.  This array may then be manipulated and used elsewhere in the query, such as the select projection or where clause. 

**Note:** Nests can be performed across multiple buckets, so long as the buckets are all on the same cluster.

##Nests and Keys
In N1QL, all nest operations must be done using document keys.  The documents on the left hand side of the nest must provide an array of document keys, and these keys are matched against the document keys on the right hand side of the join.  You may not nest against document properties on the right hand side of the nest, so it is important to take this into consideration when designing your data model.

Note: The examples below use the beer-sample bucket, and assume that brewery documents have an array of keys of beer documents.  This isn't actually the case in the default beer-sample bucket.  If you want to run these examples, you'll need to modify the brewery documents to support it.

##Inner Nests
An inner nest requires that there be at least one matching document on the right hand side of the nest.  If a matching document is not found, because none of the documents exist or the array of keys is empty or null, the document on the left hand side of the nest is dropped from the result set.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = context.Query<Brewery>()
						.Nest(
							context.Query<Beer>(),                    // documents to be nested
							brewery => brewery.Beers,                 // array of document keys from left side
							(brewery, beers) => new {brewery, beers}  // select projection for Nest
						);

			foreach (var doc in query) {
				// do work

				// each returned "document" has a brewery property
				// and a beers property that is an array of beers from that brewery

				// inner nest will not return breweries with no beers
			}
		}
	}

Note: You may apply other LINQ operations such as where and orderby clauses after the nest operation.

##Left Outer Nests
A left outer nest returns all documents on the left side of the nest, even if there are no documents on the right side is not found.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = context.Query<Brewery>()
						.LeftOuterNest(
							context.Query<Beer>(),                    // documents to be nested
							brewery => brewery.Beers,                 // array of document keys from left side
							(brewery, beers) => new {brewery, beers}  // select projection for Nest
						);

			foreach (var doc in query) {
				// do work

				// each returned "document" has a brewery property
				// and a beers property that is an array of beers from that brewery

				// may return breweries with no beers
			}
		}
	}

##Compound Keys
It is also possible to build the key on the left hand side of the nest using multiple properties and string constants.  As an example, imagine a bucket with this data structure:

	Key: "order-1001"
	{
		"type": "order",
		"orderId": 1001,
		"customerId": 5,
		"amount":123.45
	}

	Key: "customer-5"
	{
		"type": "customer",
		"customerId": 5,
		"name":"John Doe",
		"orders":[
			"1001",
			"1002",
			"1003"
		]
	}

As you can see, the key "order-1001" is not present on the customer document.  However, you may still nest the orders using a compound key.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = context.Query<Customer>()
						.Nest(
							context.Query<Order>(),                                // documents to be nested
							customer => customer.Orders.Select(p => "order-" + p), // array of document keys from left side
							(customer, orders) => new {customer, orders}           // select projection for Nest
						);

			foreach (var doc in query) {
				// do work
			}
		}
	}