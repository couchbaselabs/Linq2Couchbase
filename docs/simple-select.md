 # Controlling output with Select

> [NOTE]
> The documetation has been updated to reflect that the product name for N1QL has been changed to SQL++, however, the source itself may still use the name N1QL.

The select clause produces the result of the query and allows you to shape the format of each element.

## Simple Select

The simplest select is to return a document that maps directly to a POCO:

```cs
var beers = from b in context.Query<Beer>()
            select b;
```

This will emit query that looks something like this:

```sql
SELECT RAW `beer-sample` FROM `beer-sample` WHERE type='beer'
```

Assuming the Beer class has a document filter; if not the predicate would be removed and the entire keyspace will be returned. Any returning fields from the * will be mapped to the Properties of Beer unless a match cannot be made. If a match cannot be made, the value will be ignored.

## Selecting a subset of a document's fields

If you wish to constrain the output, you specify which fields you want returned:

```cs
var beers = from b in context.Query<Beer>()
    select new
    {
        name = b.Name,
        abv = b.Abv
    };
```

This will return only the Name and Abv fields for each document in the bucket. The result will be a projection of the original type - an anonymous type. Note that the execution of the query is deferred until the query is iterated over in foreach loop. This behavior is consistent and similar to the behavior of other LINQ providers.
