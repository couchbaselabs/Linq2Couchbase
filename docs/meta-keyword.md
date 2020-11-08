# The META() function

For every document Couchbase Server will also store another document containing the metadata describing that document. Metadata includes the following fields:

- Id - the primary key for the document
- CAS - Compare and Swap value
- Flags - SDK specific type encoding (internal use only)
- Type - the type of document stored in Couchbase (will always be JSON for N1QL)

There is additionally metadata stored within couchbase for each document (TTL and Sequence Number), but this is not available from the META function.

## META() example

The simplest usage of the META() function is to query it directly:

```cs
var query = (from meta in context.Query<DocumentMetadata>()
             select N1QlFunctions.Meta(meta)).
             Take(1);
```

In this example, we are simply retrieving the metadata from the first document returned in the beer-sample bucket. The "raw" results would look something like this:

```json
{
    //...

    "results": [
        {
            "cas": 1.4454516563790397e+18,
            "flags": 0,
            "id": "21st_amendment_brewery_cafe-south_park_blonde",
            "type": "json"
        }
    ]

    //...
}
```

Here is another example where for returning the metadata along with a field from a document and create a projection that combines the two:

```cs
var beers = (from b in context.Query<Beer>()
             where b.Type == "beer"
             select new {name = b.Name, meta = N1QlFunctions.Meta(b)});
```

And another example for returning the "id" portion of the metadata in your projection:

```cs
var beers = (from b in context.Query<Beer>()
             where b.Type == "beer"
             select new {name = b.Name, id = N1QlFunctions.Meta(b).Id});
```

The important thing to remember about the META function is that it is another tool available to you use in your queries!
