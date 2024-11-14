# The BucketContext

> [NOTE]
> The documetation has been updated to reflect that the product name for N1QL has been changed to SQL++, however, the source itself may still use the name N1QL.

The public API for Linq2Couchbase is the BucketContext; this object is similar to the DbContext in Linq2SQL and the DataContext from the EntityFramework. It's primary purpose is to provide and interface for building and submitting queries to a Couchbase server Bucket. Internally, the BucketContext uses a Cluster object and CouchbaseBucket to handle communication and to send queries and updates to the server.

## Creating a BucketContext

The BucketContext has a dependency on ICluster and IBucket objects; in your application you will need to have instantiated and initialized a Cluster object before you can create a BucketContext.

```cs
IBucket bucket = await bucketProvider.GetBucketAsync();

var context = new BucketContext(bucket);
```

It's important to note that the ICluster and IBucket objects are a long-lived objects, so you will usually want to create a singleton per application and reuse it over the lifespan of the application. A BucketContext is slightly different; it contains no Dispose method and is more ephemeral compared to the Cluster.

## Extending BucketContext

To extend BucketContext into a more powerful tool exposing strongly-typed document sets, inherit from `BucketContext` and add properties that return `IDocumentSet<T>`. These properties will be automatically initialized with an appropriate object for running queries.

```cs
public class MyContext : BucketContext
{
    // Adding "= null!" is only necessary if nullable reference types is enabled
    public IDocumentSet<Airline> Airlines { get; set; } = null!;
    public IDocumentSet<Airport> Airports { get; set; } = null!;
    public IDocumentSet<Route> Routes { get; set; } = null!;

    public MyContext(IBucket bucket) : base(bucket)
    {
    }
}
```
