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
