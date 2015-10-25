JOINing Documents
=================
Joins are used to combine multiple documents with a common link into a single query result.  They work much like traditional joins in SQL, though there are some differences.

Note: Joins can be performed across multiple buckets, so long as the buckets are all on the same cluster.

Joins and Keys
==============
In N1QL, all join operations must be done using document keys.  The documents on the left hand side of the join must provide a document key, and it is matched against the document keys on the right hand side of the join.  You may not join against document properties on the right hand side of the join, so it is important to take this into consideration when designing your data model.

Joining against document keys is represented in LINQ using N1QlFunctions.Key on the right hand side of the join equality operator.  Examples of this are included in the sections below.

Inner Joins
===========
An inner join requires that there be a matching document on the right hand side of the join.  If the matching document is not found, the document on the left hand side of the join is dropped from the result set.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = from beer in context.Query<Beer>()
						join brewery in context.Query<Brewery>()
						on beer.BreweryId equals N1QlFunctions.Key(brewery)
						select new {beerName = beer.Name, breweryName = brewery.Name};

			foreach (var doc in query) {
				// do work
				// will only have documents where the brewery exists
			}
		}
	}

Left Outer Joins
================
A left outer join returns all documents on the left side of the join, even if the document on the right side is not found.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = from beer in context.Query<Beer>()
						join brewery in context.Query<Brewery>()
						on beer.BreweryId equals N1QlFunctions.Key(brewery) into breweryGroup
						from brewery in breweryGroup.DefaultIfEmpty()
						select new {beerName = beer.Name, breweryName = brewery.Name};

			foreach (var doc in query) {
				// do work
				// will have all beers, even if the brewery document doesn't exist
				// if the brewery document doesn't exist, breweryName will be null
			}
		}
	}

Compound Keys
=============
It is also possible to build the key on the left hand side of the join using multiple properties and string constants.  As an example, imagine a bucket with this data structure:

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
		"name":"John Doe"
	}

As you can see, the key "customer-5" is not present on the order document.  However, you may still join these two documents together using a compound key.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = from order in context.Query<Order>()
						join customer in context.Query<Customer>()
						on "customer-" + order.CustomerId.ToString() equals N1QlFunctions.Key(customer)
						select new {order.OrderId, customer.Name};

			foreach (var doc in query) {
				// do work
			}
		}
	}