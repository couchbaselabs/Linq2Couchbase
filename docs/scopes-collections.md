# Scopes and Collections

Beginning with Couchbase Server 7.0, querying against documents stored in
[scopes and collections](https://docs.couchbase.com/server/current/learn/data/scopes-and-collections.html)
is fully supported. Linq2Couchbase 2.0 supports querying against documents
stored within a non-default collection.

## Default Behavior

The default behavior is to query documents located in the default collection
(scope "_default" and collection "_default"). Queries against these documents
are generated in a backward-compatible format that doesn't include the scope
or collection name. No code changes are necessary to query against these documents.

## Specifying a Collection

To specify a non-default collection, add the `CouchbaseCollection` attribute to your
POCO.

```cs
[CouchbaseCollection("my_scope", "my_collection")]
public class MyDocument
{
    // Properties here
}
```

Queries for this document will automatically account for the scope and collection,
including in joins, nests, and subqueries.

## Specifying a Collection in BucketContext

In some cases adding the CouchbaseCollection attribute to a POCO may not be an option.
In this case, the collection may also be added to `IDocumentSet<T>` properties on a class
inherited from `BucketContext`. This will take precedence over any settings on the POCO.

```cs
public class MyContext : BucketContext
{
    [CouchbaseCollection("inventory", "route")]
    public IDocumentSet<Route> Routes { get; set; }

    public MyContext(IBucket bucket) : base(bucket)
    {
    }
}
```

See [Bucket Context](./bucket-context.md) for more details.

## DocumentTypeFilter

For previous versions of Linq2Couchbase, a very common pattern was to apply the
`DocumentTypeFilter` attribute to POCOs to filter queries based on the `type`
attribute. This attribute will still work in combination with scopes and collections.

However, with collections it is often unnecessary and removing the attribute may improve
performance. This is because a collection generally only has a single document type,
making additional filtering redundant. Therefore, if your collection only has a single
document type, the recommended practice is to not use `DocumentTypeFilter` attributes.

> :info: Note that you must also remove any `WHERE type = 'x'` predicates from your
> index definitions.
