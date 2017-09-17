# Result Streaming
By default, Linq2Couchbase loads the entire result set into memory before returning the result to you.  However, for very large result sets this can result in high memory utilization.  Result streaming can be used to reduce the memory footprint for these queries.

## Caveats
- Result streaming is incompatible with custom deserializers.  Results will always be streamed using Newtonsoft.Json.
- Result streaming is always disabled when [change tracking](change-tracking.md) is enabled, even if you request streaming.
- The benefits of result streaming are lost if you call `.ToList()` or otherwise load the results into memory yourself.  Instead, you should always iterate the collection, only using items from the collection for the lifetime of the iteration step.

## Usage
The call to UseStreaming should be immediately after the call to Query<T>.  It is only required on the first call if performing joins.

```csharp
using (var cluster = new Cluster())
{
    using (var bucket = cluster.OpenBucket("beer-sample"))
    {
        var context = new BucketContext(bucket);

        var query = from beer in context.Query<Beer>().UseStreaming(true)
                    select beer;

        foreach (var beer in query)
        {
            // Do something here, but don't save beer outside of the loop
        }
    }
}
```