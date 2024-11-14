# Unnesting Documents

> [NOTE]
> The documetation has been updated to reflect that the product name for N1QL has been changed to SQL++, however, the source itself may still use the name N1QL.

Unnesting documents is somewhat similar to performing a join.  However, instead of operating against documents in a bucket, it joins the main body of the document to subdocuments on arrays inside the main body of the document.  The main body of the document is repeated in the result set for each subdocument.

## Inner Unnests

An inner unnest requires that there be at least one subdocument on the right side of the unnest.  If the array being unnested is null or has no subdocuments, the main document is excluded from the result set.

```cs
var context = new BucketContext(bucket);

var query = from brewery in context.Query<Brewery>()
            from address in brewery.Address
            select new {brewery.Name, AddressLine = address};

await foreach (var doc in query.AsAsyncEnumerable()) {
    // do work

    // each returned "document" has a the brewery name property
    // and one of the address lines from the brewery

    // if the brewery has more than one address line, the brewer name
    // will be repeated for each address line

    // inner unnest will not return breweries with no address lines
}
```

## Left Outer Nests

A left outer unnest returns all documents on the left side of the unnest, even if the array being unnested is null or has no subdocuments.

```cs
var context = new BucketContext(bucket);

var query = from brewery in context.Query<Brewery>()
            from address in brewery.Address.DefaultIfEmpty()
            select new {brewery.Name, AddressLine = address};

foreach (var doc in query) {
    // do work

    // each returned "document" has a the brewery name property
    // and one of the address lines from the brewery

    // if the brewery has more than one address line, the brewer name
    // will be repeated for each address line

    // left outer unnest will return breweries with no address lines
    // if there are no address lines, AddressLine will be null
}
```
