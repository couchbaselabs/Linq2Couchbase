# Sorting and Paging Results

> [NOTE]
> The documetation has been updated to reflect that the product name for N1QL has been changed to SQL++, however, the source itself may still use the name N1QL.

Using LINQ and SQL++, it's easy to sort and page results.

## Sorting

For LINQ, sorting of results is represented by the orderby clause, which accepts a comma-delimited list of properties to sort by.

```cs
var context = new BucketContext(bucket);

var query = from contact in context.Query<Contact>()
            orderby contact.LastName, contact.FirstName
            select contact;

await foreach (var doc in query.AsAsyncEnumerable()) {
    // do work
    // query will return the contacts sorted by last name, then by first name
}
```

To sort in descending order, add the descending keyword to the sort property.

```cs
var context = new BucketContext(bucket);

var query = from contact in context.Query<Contact>()
            orderby contact.LastName descending, contact.FirstName descending
            select contact;

await foreach (var doc in query.AsAsyncEnumerable()) {
    // do work
    // query will return the contacts sorted by last name descending, then by first name descending
}
```

## Paging

Paging of data is done using the LINQ Skip and Take functions. Skip is applied first, and skips a certain number of records in the result set. Then Take is applied, which limits the maximum number of results which are returned.

When possible, paging using Skip and Take can significantly improve query performance by allowing the query to short-circuit once enough data is available for the result set.

Additionally, Take and Skip should normally be used in conjunction with an orderby clause.  Without sorting the query results become unpredictable.

```cs
var context = new BucketContext(bucket);

var query = (from contact context.Query<Contact>()
             orderby contact.LastName, contact.FirstName
             select contact).Skip(20).Take(10);

await foreach (var doc in query.AsAsyncEnumerable()) {
    // do work
    // query will return the contacts sorted by last name, then by first name
    // returning only the 21st through 30th contacts (if any)
}
```

> :info: **Note:** It is not required to use both Skip and Take, you may use one without the other.
