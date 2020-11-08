# The BucketContext

The public API for Linq2Couchbase is the BucketContext; this object is similar to the DbContext in Linq2SQL and the DataContext from the EntityFramework. It's primary purpose is to provide and interface for building and submitting queries to a Couchbase server Bucket. Internally, the BucketContext uses a Cluster object and CouchbaseBucket to handle communication and to send queries and updates to the server.

## Creating a BucketContext

The BucketContext has a dependency on ICluster and IBucket objects; in your application you will need to have instantiated and initialized a Cluster object before you can create a BucketContext.

```cs
IBucket bucker = await bucketProvider.GetBucketAsync();

var context = new BucketContext(bucket);
```

It's important to note that the ICluster and IBucket objects are a long-lived objects, so you will usually want to create a singleton per application and reuse it over the lifespan of the application. A BucketContext is slightly different; it contains no Dispose method and is more ephemeral compared to the Cluster.
