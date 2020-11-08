# Any and All

The LINQ Any and All methods are a useful way to test for a condition across a series of documents.

## Any

Any returns true if any of the documents in the list meet the criteria provided.  If no criteria is provided, then it will return true so long as there is at least one document in the list.

```cs
var context = new BucketContext(bucket);

var result = await context.Query<Beer>().AnyAsync();

// result is true if there are any beers in the bucket
}
```

Or:

```cs
var context = new BucketContext(bucket);

var result = await context.Query<Beer>().AnyAsync(beer => beer.Abv >= 6);

// result is true if there are any beers with an Abv >= 6
```

```cs
You may also apply Any at the end of a more complex query:

var context = new BucketContext(bucket);

var result = await (from beer in context.Query<Beer>()
                join brewery in context.Query<Brewery>()
                on beer.BreweryId equals N1QlFunctions.Key(brewery)
                select new {brewery.State}
                ).AnyAsync(p => p.State == "MI");

// result is true if there are any beers made in Michigan
```

## All

All returns true only if all of the documents meet the criteria provided.

```cs
var context = new BucketContext(bucket);

var result = await (from beer in context.Query<Beer>()
                where beer.BreweryId = "21st_amendment_brewery_cafe"
                select beer
                ).AllAsync(beer => beer.Abv >= 6);

// result is true if all beers from 21st Amendment Brewery Cafe have an Abv >= 6
```

## Arrays

Any and All operations may also be applied to arrays within documents, either as part of the where clause or the select projection.

```cs
var context = new BucketContext(bucket);

var query = from brewery in context.Query<Brewery>()
            where brewery.Address.Any()
            select brewery;

// query will return all breweries with at least one address line
```

```cs
var context = new BucketContext(bucket);

var query = from brewery in context.Query<Brewery>()
            where brewery.Address.Any()
            select new {brewery.Name, hasAddress = brewery.Address.Any()};

// query will return the brewery name and a boolean indicating if it has at least one address line
```
