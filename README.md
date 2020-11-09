# Linq2Couchbase

[![Join the chat at https://gitter.im/couchbaselabs/Linq2Couchbase](https://badges.gitter.im/couchbaselabs/Linq2Couchbase.svg)](https://gitter.im/couchbaselabs/Linq2Couchbase?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
![.NET Core](https://github.com/couchbaselabs/Linq2Couchbase/workflows/.NET%20Core/badge.svg)

The official Language Integrated Query (LINQ) provider for querying Couchbase Server with [N1QL](https://docs.couchbase.com/server/current/n1ql/n1ql-intro/queriesandresults.html) using the Couchbase .NET SDK. The goal of Linq2Couchbase is to create a lightweight ORM/ODM for querying Couchbase Buckets using LINQ as the lingua-franca between your application and Couchbase Server using N1QL, a SQL-like query language for JSON documents. It also provides a write API for performing CRUD operations on JSON documents.

While not an officially supported Couchbase project, this repo is actively maintained and monitored. If you happen to find a bug or have any questions, please either create an [issue](https://github.com/couchbaselabs/linq2couchbase/issues) or make a post on [forums.couchbase.com](https://forums.couchbase.com/c/net-sdk). Additionally, we actively accept contributions!

> :info: This documentation is for Linq2Couchbase 2.x, compatible with Couchbase SDK 3.x. For documentation on
> Linq2Couchbase 1.x compatible with Couchbase SDK 2.x, see [the release14 branch](https://github.com/couchbaselabs/Linq2Couchbase/blob/release14/README.md).

## Getting started

The Linq2Couchbase project has the following dependencies:

- Couchbase Server 5.5 or greater with the query service enabled on at least one node
- Couchbase .NET SDK 3.0.3 or greater

### Installing Couchbase Server

For a single instance of Couchbase Server running on localhost, you can [download Couchbase Server here](https://www.couchbase.com/downloads) (make sure it's 4.0 or later). If you would like to create a cluster, the easiest way is by using the [Vagrant scripts for provisioning clusters](https://github.com/couchbaselabs/vagrants). Additionally, you can use [Docker scripts](https://hub.docker.com/r/couchbase/server/). Follow the directions on each respective link for installation information.

### Installing the package using NuGet

Once you have a Couchbase Server instance or cluster setup, open Visual Studio 2019 or greater or [MonoDevelop](http://www.monodevelop.com/) and create a .NET or .NET Core application. Open the NuGet Package Manager and search for "Couchbase Linq" or type the following into the Package Manager console:

```powershell
Install-Package Linq2Couchbase
```

## Quick Start

This Quick Start assumes that you have [already installed the travel-sample bucket](https://docs.couchbase.com/server/current/manage/manage-settings/install-sample-buckets.html), which is available and built-in to Couchbase Server.

Query the 'travel-sample' bucket and return 10 airlines in any order:

```cs
static async void Main() {
    var cluster = await Cluster.ConnectAsync("couchbase://localhost", options => {
        options.UserName = "Administrator";
        options.Password = "password";

        options.AddLinq();
    });

    // Wait until the cluster is bootstrapped
    await cluster.WaitUntilReadyAsync(TimeSpan.FromSeconds(10));

    var context = new BucketContext(await cluster.BucketAsync("travel-sample"));
    var query = (from a in context.Query<AirLine>()
                where a.Country == "United Kingdom"
                select a)
                .Take(10);

    await foreach (var airline in query.ToAsyncEnumerable())
    {
        Console.WriteLine(airline.Name);
    }
}
```

Upon running this query, you should expect console output similar to:

```json
{"Id":"10642","Type":"airline","Name":"Jc royal.britannica","Iata":null,"Icao":"JRB","Callsign":null,"Country":"United Kingdom"}
{"Id":"112","Type":"airline","Name":"Astraeus","Iata":"5W","Icao":"AEU","Callsign":"FLYSTAR","Country":"United Kingdom"}
...
{"Id":"16881","Type":"airline","Name":"Air Cudlua","Iata":null,"Icao":"CUD","Callsign":"Cudlua","Country":"United Kingdom"}
```

## ASP.NET Core Quick Start

For use with ASP.NET Core, it is recommended to use the [Couchbase.Extensions.DependencyInjection](https://www.nuget.org/packages/Couchbase.Extensions.DependencyInjection/) NuGet package to register Couchbase with DI. Documentation can be [found here](https://docs.couchbase.com/dotnet-sdk/current/howtos/managing-connections.html). In that case, the bootstrap code in `Startup.cs` may look like this:

```cs
public void ConfigureServices(IServiceCollection services)
{
    // Other configuration here

    services
        .AddCouchbase(options => {
            Configuration.GetSection("Couchbase").Bind(options);
            options.AddLinq();
        })
        .AddCouchbaseBucket<INamedBucketProvider>("bucket-name");
}

```

A `BucketContext` may then be created in a controller:

```cs
public class MyController : ControllerBase
{
    private readonly INamedBucketProvider _bucketProvider;

    public MyController(INamedBucketProvider bucketProvider)
    {
        _bucketProvider = bucketProvider;
    }

    public async Task<IActionResult> Get()
    {
        var bucketContext = new BucketContext(await _bucketProvider.GetBucketAsync());

        // Use bucket context here
    }
}
```

## Developer Guide

- [The BucketContext: how to use with ASP.NET and Owin/Katana applications](docs/bucket-context.md)
- [Mapping JSON fields to POCO properties](docs/poco-mapping.md)
- [Mapping JSON documents to POCOs with DocumentFilters](docs/document-filters.md)
- [Controlling output with Select](docs/simple-select.md)
- [Filtering with Where](docs/where-clause.md)
- [Sorting and Paging Results](docs/sorting-take-limit.md)
- [String Handling](docs/string-handling.md)
- [Math Functions](docs/math-functions.md)
- [Date Handling](docs/date-handling.md)
- [Array Filtering, Projections, and Sorting](docs/array-filtering-projections.md)
- [Grouping and Aggregation](docs/grouping-aggregation.md)
- [The UseKeys Method](docs/use-keys.md)
- [Query Hints](docs/query-hints.md)
- [Joining Documents](docs/joins.md)
- [Nesting Documents](docs/nest.md)
- [Unnesting Documents](docs/unnest.md)
- [Any and All](docs/any-all.md)
- [Testing For NULL And MISSING Attributes](docs/null-missing-valued.md)
- [The META Keyword](docs/meta-keyword.md)
- [Working With Enumerations](docs/enum.md)
- [Asynchronous Queries](docs/async-queries.md)
- [Serialization Converters](docs/serialization-converters.md)
- [Custom JSON Serializers](docs/custom-serializers.md)
- [Using Read Your Own Write (RYOW) Consistency](docs/ryow.md)

## Building From Source

Linq2Couchbase uses the NuGet package manager for handling dependencies.  To build from the source, simply clone the GitHub repository and build in Visual Studio.  The NuGet package manager should download all required dependencies.

## Project management

In the [Jira project for Linq2Couchbase](http://issues.couchbase.com/browse/LINQ), you can file bugs, propose features or get an idea for the roadmap there. There is also a [list of supported and proposed N1QL features for Linq2Couchbase](https://docs.google.com/document/d/1hPNZ-qTKpVzQsFwg_1uUueltzNL1wA75L5F-hYF92Cw/edit?usp=sharing).

## Contributors

Linq2Couchbase is an open source project and community contributions are welcome whether they be pull-requests, feedback or filing bug tickets or feature requests. We appreciate any contribution no matter how big or small! If you do decide to contribute, please browse the Jira project and ensure that that feature or issue hasn't already been documented. If you want to work on a feature, bug or whatever please create or select a ticket and set the status to "in-progress".
