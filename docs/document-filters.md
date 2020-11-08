# Mapping JSON documents to POCOs with DocumentFilters

Couchbase allows you store heterogeneous documents within a Bucket or "keyspace". When you do a select without a predicate, you are querying across the entire keyspace. For example, imagine a bucket with various document "types" and you do a N1QL query like this:

```sql
SELECT name FROM `travel-sample`
```

This query will return the name field for every document in the bucket. It's more likely you would want to constrain the query to a subset of the entire bucket, returning results from only one kind of document, so you would do something like this:

```sql
SELECT name FROM `travel-sample` WHERE type="airline"
```

In your application, this would translate to a LINQ query that maps the document to an C# object with strongly typed fields matching the document:

```cs
var query = from x in db.Query<Airline>()
    where x.Type == "airline"
    select x.Name;
```

This is effective, but becomes a but tedious to do for every query. Instead, it would be much simpler to remove the explicit Type predicate from the query:

```cs
var query = from x in db.Query<Airline>()
    select x.Name;
```

This is exactly what Document Filters allow you to do: map a JSON document to a C# POCO without explicitly (and redundantly) having to provide a predicate (WHERE) based on the "type" attribute. Instead you either create a custom DocumentFilter or you use a DocumentTypeFilterAttribute and you associate it with your C# class or POCO.

## Using the DocumentTypeFilterAttribute

Assuming we have a document that looks like this:

```json
{
    "id": 24,
    "type": "airline",
    "name": "American Airlines",
    "iata": "AA",
    "icao": "AAL",
    "callsign": "AMERICAN",
    "country": "United States"
}
```

We can have an associated Airline class that is symmetrical in form and shape:

```cs
[DocumentTypeFilter("airline")]
public class Airline
{
    [JsonIgnore]
    public string Id { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("iata")]
    public string Iata { get; set; }

    [JsonProperty("icao")]
    public string Icao { get; set; }

    [JsonProperty("callsign")]
    public string Callsign { get; set; }

    [JsonProperty("country")]
    public string Country { get; set; }
}
```

Now when we generate a LINQ query, the `WHERE type="airline"` clause will be automatically added to the emitted N1QL query and only documents of type "airline" will be returned.

## Custom Document Filters

In addition to the default filters, custom filters are also supported which enable you to apply any predicate you wish to a POCO. To use a custom document filter, you create an implementation of IDocumentFilter which matches the criterion you wish to filter by.
Here is an example of a custom filter which filters by Type:

```cs
class BreweryFilter : IDocumentFilter<Brewery>
{
    public int Priority { get; set; }

    public IQueryable<Brewery> ApplyFilter(IQueryable<Brewery> source)
    {
        return source.Where(p => p.Type == "brewery");
    }
}
```

Note that the predicate can be anything you wish, even compound.

There are two options for applying a custom document filter.  The first is to make a custom filter attribute inherited from `DocumentFilterAttribute`. Then apply this attribute to the document types.

The second is to register the filter globally when building `ClusterOptions`.

```cs
services.AddCouchbase(options => {
    options.AddLinq(linqOptions => linqOptions.DocumentFilterManager.SetFilter<DocumentType>(new BreweryFilter());
});
```

For an example of a dynamic document filter attribute inherited from DocumentFilterAttribute, see the source code for [DocumentTypeFilterAttribute](../Src/Couchbase.Linq/Filters/DocumentTypeFilterAttribute.cs).
