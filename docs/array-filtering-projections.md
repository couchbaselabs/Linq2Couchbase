# Array Filtering, Projections, and Sorting

Using array filtering and projections, you can alter the nature of an array located inside a document before it is returned by N1QL.

## Filtering

To filter an array, simply apply a where clause to the array inside the select projection.  For .Net type consistency, you should apply a ToArray() or ToList() after the subquery.

```cs
var context = new BucketContext(bucket);

var query = from brewery in context.Query<Brewery>()
            select new {
                brewery.Name,
                longAddresses = (from address in brewery.Address
                                    where address.Length > 30
                                    select address).ToList()
            };

await foreach (var doc in query.AsAsyncEnumerable()) {
    // do work
    // query will return the brewery name and a list of addresses more than 30 chars long
}
```

## Projections

Arrays may also be projected to alter the contents of the array.  To perform a projection, simply include it in the select clause of the subquery.

```cs
var context = new BucketContext(bucket);

var query = from brewery in context.Query<Brewery>()
            select new {
                brewery.Name,
                addresses = (from address in brewery.Address
                             select address.Substring(1)).ToList()
            };

await foreach (var doc in query.AsAsyncEnumerable()) {
    // do work
    // query will return the brewery name and a list of the first character of each address line
}
```
