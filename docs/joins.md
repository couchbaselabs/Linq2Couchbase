JOINing Documents
=================
Joins are used to combine multiple documents with a common link into a single query result.  They work much like traditional joins in SQL, though there are some differences.

**Note:** Joins can be performed across multiple buckets, so long as the buckets are all on the same cluster.

##Joins and Keys
In N1QL on Couchbase 5.1 and earlier, all join operations must be done using document keys.  The documents on the left hand side of the join must provide a document key, and it is matched against the document keys on the right hand side of the join.  You may not join against document properties on the right hand side of the join, so it is important to take this into consideration when designing your data model.

Joining against document keys is represented in LINQ using N1QlFunctions.Key on the right hand side of the join equality operator.  Examples of this are included in the sections below.

##Inner Joins
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

##Left Outer Joins
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

##Compound Keys
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

##Index Joins

Beginning with Couchbase Server 4.5, it is possible to perform joins where the key is stored in the document on the right side of the join.  Previously, the key had to be stored on the document on the left side of the join.

This kind of nest operation is actually more consistent with LINQ standards, and is represented by the group join construct.  The requirement is that the left hand side of the join equality operator must use N1QlFunctions.Key to get the key from one of the other extents in the query. 

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = from brewery in context.Query<Brewery>()
						join brewery in context.Query<Beer>()
						on N1QlFunctions.Key(brewery) equals beer.BreweryId
						where brewery.Name == "21st Century"
						select new {beerName = beer.Name};

			foreach (var doc in query) {
				// do work
			}
		}
	}

Since indexes are only used on the left most document, this approach to joins can improve performance significantly by allowing the use of an index on the document that doesn't have the key for the join.

In order to perform this type of join, there must be an index created on the key field in the child document.  In the example above, the index must be on the BreweryId field.  More information about this type of join operation can be found [here](http://developer.couchbase.com/documentation/server/4.5-dp/flexible-join-n1ql.html).

Note that a NotSupportedException will be thrown if you execute this kind of join operation against a 4.0 or 4.1 Couchbase cluster.

## ANSI Joins

Beginning with Couchbase Server 5.5, N1QL supports full ANSI joins. It is now possible to join against any properties on either side, the N1QlFunctions.Key limitation no longer applies.

To use this feature, simply join on the desired properties in LINQ. It is necessary, however, to ensure there is an index which can be used to lookup the properties on the right-hand side. Attempting to use this feature on a Couchbase Server cluster before version 5.5 will result in a NotSupportedException.

```cs
var context = new BucketContext(bucket);

var query = from route in context.Query<Route>()
            join airport in context.Query<Airport>()
            on route.DestinationAirport equals airport.Faa
            select new {airport.AirportName, route.Airline};

foreach (var doc in query)
{
    // do work
}
```