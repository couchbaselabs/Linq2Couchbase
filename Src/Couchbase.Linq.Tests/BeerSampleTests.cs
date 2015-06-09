using System;
using System.Collections.Generic;
using System.Linq;
using Couchbase.Configuration.Client;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Tests.Documents;
using NUnit.Framework;

namespace Couchbase.Linq.Tests
{
    [TestFixture]
    public class BeerSampleTests : N1QLTestBase
    {
        [SetUp]
        public void TestSetUp()
        {
            Filters.EntityFilterManager.Clear();   
        }

        [Test]
        public void Map2PocoTests()
        {
            var clientConfiguration = new ClientConfiguration
            {
                Servers = new List<Uri>
                {
                    new Uri("http://localhost:8091")
                }
            };
            using (var cluster = new Cluster())
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = from b in bucket.Queryable<Beer>()
                        select b;

                    foreach (var beer in beers)
                    {
                        Console.WriteLine(beer.Name);
                    }
                }
            }
        }

        [Test]
        public void Map2PocoTests_Simple_Projections()
        {
            using (var cluster = new Cluster())
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = from b in bucket.Queryable<Beer>()
                        select new {name = b.Name, abv = b.Abv};

                    foreach (var b in beers)
                    {
                        Console.WriteLine("{0} has {1} ABV", b.name, b.abv);
                    }
                }
            }
        }

        [Test]
        public void Map2PocoTests_Simple_Projections_Where()
        {
            using (var cluster = new Cluster())
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = from b in bucket.Queryable<Beer>()
                        where b.Type == "beer"
                        select new {name = b.Name, abv = b.Abv};

                    foreach (var b in beers)
                    {
                        Console.WriteLine("{0} has {1} ABV", b.name, b.abv);
                    }
                }
            }
        }

        [Test]
        public void Map2PocoTests_Simple_Projections_Limit()
        {
            using (var cluster = new Cluster())
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = (from b in bucket.Queryable<Beer>()
                        where b.Type == "beer"
                        select new {name = b.Name, abv = b.Abv}).
                        Take(10).
                        Skip(5);

                    foreach (var b in beers)
                    {
                        Console.WriteLine("{0} has {1} ABV", b.name, b.abv);
                    }
                }
            }
        }

        [Test]
        public void Map2PocoTests_Simple_Projections_TypeFilterAttribute()
        {
            using (var cluster = new Cluster())
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = (from b in bucket.Queryable<BeerFiltered>()
                                 select new { type = b.Type }).
                                 AsEnumerable();

                    Assert.True(beers.All(p => p.type == "beer"));
                }
            }
        }

        [Test]
        public void Map2PocoTests_Simple_Projections_TypeFilterRuntime()
        {
            Filters.EntityFilterManager.SetFilter(new BreweryFilter());

            using (var cluster = new Cluster())
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var breweries = (from b in bucket.Queryable<Brewery>()
                                     select new { type = b.Type })
                                     .AsEnumerable();

                    Assert.True(breweries.All(p => p.type == "brewery"));
                }
            }
        }
    }
}