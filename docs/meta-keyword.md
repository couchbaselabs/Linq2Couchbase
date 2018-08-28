The META() function
================
For every document Couchbase Server will also store another document containing the meta data describing that document. Meta-data includes the following fields:

- Id - the primary key for the document
- CAS - Compare and Swap value
- Flags - SDK specific type encoding (internal use only)
- Type - the type of document stored in Couchbase (will always be JSON for N1QL)

There is additionally meta-data stored within couchbase for each document (TTL and Sequence Number), but this is not available from the META function. 

## META() example
The simplest usage of the META() function is to query it directly:

    var query = (from meta in context.Query<DocumentMetadata>()
                 select N1QlFunctions.Meta(meta)).
				 Take(1);

In this example, we are simply retrieving the meta-data from the first document returned in the beer-sample bucket. The "raw" results would look something like this:

    {
	    ...

	    "results": [
	        {
	             {
	                "cas": 1.4454516563790397e+18,
	                "flags": 0,
	                "id": "21st_amendment_brewery_cafe-south_park_blonde",
	                "type": "json"
	            }
	        }
	    ]

        ...
	}

Here is another example where you want to return the meta-data along with a field from a document and create a projection that combines the two:

	 var beers = (from b in context.Query<Beer>()
                  where b.Type == "beer"
                  select new {name = b.Name, meta = N1QlFunctions.Meta(b)});

And another example where you only want to return the "id" portion of the meta-data in your projection:

	var beers = (from b in context.Query<Beer>()
                 where b.Type == "beer"
                 select new {name = b.Name, id = N1QlFunctions.Meta(b).Id});

The important thing to remember about the META function is that it is another tool available to you use in your queries!



