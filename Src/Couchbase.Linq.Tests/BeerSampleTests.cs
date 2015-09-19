using System;
using System.Collections.Generic;
using System.Linq;
using Couchbase.Configuration.Client;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Filters;
using Couchbase.Linq.Tests.Documents;
using Couchbase.N1QL;
using NUnit.Framework;

namespace Couchbase.Linq.Tests
{
    [TestFixture]
    public class BeerSampleTests : N1QLTestBase
    {
        protected override bool IsClusterRequired
        {
            get { return true; }
        }

        [SetUp]
        public void TestSetUp()
        {
            Filters.EntityFilterManager.Clear();
        }

        [Test]
        public void Map2PocoTests()
        {
            using (var cluster = new Cluster(
                TestConfigurations.DefaultConfig()))
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
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
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
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
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
        public void Map2PocoTests_Simple_Projections_WhereNot()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = from b in bucket.Queryable<Beer>()
                        where b.Type == "beer" && !(b.Abv < 4)
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
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
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
        public void Map2PocoTests_Simple_Projections_Meta()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = (from b in bucket.Queryable<Beer>()
                        where b.Type == "beer"
                        select new {name = b.Name, meta = N1Ql.Meta(b)}).
                        Take(10);

                    foreach (var b in beers)
                    {
                        Console.WriteLine("{0} has metadata {1}", b.name, b.meta);
                    }
                }
            }
        }

        [Test]
        public void Map2PocoTests_Explain()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var explanation = (from b in bucket.Queryable<Beer>()
                        where b.Type == "beer"
                        select b).
                        Explain();

                    Console.WriteLine(explanation);
                }
            }
        }

        [Test]
        public void Map2PocoTests_NewObjectsInArray()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var query = from brewery in bucket.Queryable<Brewery>()
                        where brewery.Type == "brewery"
                        select
                            new
                            {
                                name = brewery.Name,
                                list =
                                    new[]
                                    {new {part = brewery.City}, new {part = brewery.State}, new {part = brewery.Code}}
                            };

                    foreach (var brewery in query.Take(10))
                    {
                        Console.WriteLine("Brewery {0} has address parts {1}", brewery.name,
                            String.Join(", ", brewery.list.Select(p => p.part)));
                    }
                }
            }
        }

        [Test]
        public void UseKeys_SelectDocuments()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var query =
                        from brewery in
                            bucket.Queryable<Brewery>().UseKeys(new[] {"21st_amendment_brewery_cafe", "357"})
                        select new {name = brewery.Name};

                    foreach (var brewery in query)
                    {
                        Console.WriteLine("Brewery {0}", brewery.name);
                    }
                }
            }
        }

        [Test]
        public void AnyAllTests_AnyNestedArrayWithFilter()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var breweries = (from b in bucket.Queryable<Brewery>()
                        where b.Type == "brewery" && b.Address.Any(p => p == "563 Second Street")
                        select new {name = b.Name, address = b.Address}).
                        ToList();

                    Assert.IsNotEmpty(breweries);
                    Assert.True(breweries.All(p => p.address.Contains("563 Second Street")));

                }
            }
        }

        [Test]
        public void AnyAllTests_AnyOnMainDocument_ReturnsTrue()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var hasBreweries = (from b in bucket.Queryable<Brewery>()
                        where b.Type == "brewery"
                        select new {name = b.Name, address = b.Address}).
                        Any();

                    Assert.True(hasBreweries);
                }
            }
        }

        [Test]
        public void AnyAllTests_AnyOnMainDocument_ReturnsFalse()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var hasFaketype = (from b in bucket.Queryable<Brewery>()
                        where b.Type == "faketype"
                        select new {name = b.Name, address = b.Address}).
                        Any();

                    Assert.False(hasFaketype);
                }
            }
        }

        [Test]
        public void AnyAllTests_AllNestedArray()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var breweries = (from b in bucket.Queryable<Brewery>()
                        where b.Type == "brewery" && b.Address.All(p => p == "563 Second Street")
                        select new {name = b.Name, address = b.Address}).
                        ToList();

                    Assert.IsNotEmpty(breweries);
                    Assert.True(breweries.SelectMany(p => p.address).All(p => p == "563 Second Street"));
                }
            }
        }

        [Test]
        public void AnyAllTests_AllNestedArrayPrefiltered()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    // Note: This query isn't very useful in the real world
                    // However, it does demonstrate how to prefilter the collection before all is run
                    // Which is behaviorly different then adding the Where predicate inside the All predicate
                    // In this example, all breweries which have NO address 563 Second Street will be returned

                    var breweries = (from b in bucket.Queryable<Brewery>()
                        where
                            b.Type == "brewery" &&
                            b.Address.Where(p => p == "563 Second Street").All(p => p == "101 Fake Street")
                        orderby b.Name
                        select new {name = b.Name, address = b.Address}).
                        ToList();

                    Assert.IsNotEmpty(breweries);
                    Assert.False(breweries.SelectMany(p => p.address).Any(p => p == "563 Second Street"));
                }
            }
        }

        [Test]
        public void AnyAllTests_AllOnMainDocument_ReturnsFalse()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var isAllBreweries = bucket.Queryable<Brewery>().All(p => p.Type == "brewery");

                    Assert.False(isAllBreweries);
                }
            }
        }

        [Test]
        public void AnyAllTests_AllOnMainDocument_ReturnsTrue()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var allBreweriesHaveAddress = (from b in bucket.Queryable<Brewery>()
                        where b.Type == "brewery"
                        select new {b.Name})
                        .All(p => p.Name != "");

                    Assert.True(allBreweriesHaveAddress);
                }
            }
        }

        [Test]
        public void Map2PocoTests_Simple_Projections_TypeFilterAttribute()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = (from b in bucket.Queryable<BeerFiltered>()
                        select new {type = b.Type}).
                        AsEnumerable();

                    Assert.True(beers.All(p => p.type == "beer"));
                }
            }
        }

        [Test]
        public void Map2PocoTests_Simple_Projections_TypeFilterRuntime()
        {
            EntityFilterManager.SetFilter(new BreweryFilter());

            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var breweries = (from b in bucket.Queryable<Brewery>()
                        select new {type = b.Type})
                        .AsEnumerable();

                    Assert.True(breweries.All(p => p.type == "brewery"));
                }
            }
        }

        public void Map2PocoTests_Simple_Projections_MetaWhere()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = (from b in bucket.Queryable<Beer>()
                        where b.Type == "beer" && N1Ql.Meta(b).Type == "json"
                        select new {name = b.Name}).
                        Take(10);

                    foreach (var b in beers)
                    {
                        Console.WriteLine("{0} is a JSON document", b.name);
                    }
                }
            }
        }

        public void Map2PocoTests_Simple_Projections_MetaId()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = (from b in bucket.Queryable<Beer>()
                        where b.Type == "beer"
                        select new {name = b.Name, id = N1Ql.Meta(b).Id}).
                        Take(10);

                    foreach (var b in beers)
                    {
                        Console.WriteLine("{0} has id {1}", b.name, b.id);
                    }
                }
            }
        }

        public void AnyAllTests_AnyNestedArray()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var breweries = (from b in bucket.Queryable<Brewery>()
                        where b.Type == "brewery" && b.Address.Any()
                        select new {name = b.Name, address = b.Address}).
                        ToList();

                    Assert.IsNotEmpty(breweries);
                    Assert.True(breweries.All(p => p.address.Any()));
                }
            }
        }

        [Test]
        public void JoinTests_InnerJoin_Simple()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = from beer in bucket.Queryable<Beer>()
                        join brewery in bucket.Queryable<Brewery>()
                            on beer.BreweryId equals N1Ql.Key(brewery)
                        select new {beer.Name, beer.Abv, BreweryName = brewery.Name};

                    foreach (var b in beers.Take(10))
                    {
                        Console.WriteLine("Beer {0} with ABV {1} is from {2}", b.Name, b.Abv, b.BreweryName);
                    }
                }
            }
        }

        [Test]
        public void JoinTests_InnerJoin_SortAndFilter()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = from beer in bucket.Queryable<Beer>()
                        join brewery in bucket.Queryable<Brewery>()
                            on beer.BreweryId equals N1Ql.Key(brewery)
                        where brewery.Geo.Longitude > -80
                        orderby beer.Name
                        select new {beer.Name, beer.Abv, BreweryName = brewery.Name};

                    foreach (var b in beers.Take(10))
                    {
                        Console.WriteLine("Beer {0} with ABV {1} is from {2}", b.Name, b.Abv, b.BreweryName);
                    }
                }
            }
        }

        [Test]
        public void JoinTests_InnerJoin_Prefiltered()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = from beer in bucket.Queryable<Beer>().Where(p => p.Type == "beer")
                        join brewery in bucket.Queryable<Brewery>().Where(p => p.Type == "brewery")
                            on beer.BreweryId equals N1Ql.Key(brewery)
                        where brewery.Geo.Longitude > -80
                        orderby beer.Name
                        select new {beer.Name, beer.Abv, BreweryName = brewery.Name};

                    foreach (var b in beers.Take(10))
                    {
                        Console.WriteLine("Beer {0} with ABV {1} is from {2}", b.Name, b.Abv, b.BreweryName);
                    }
                }
            }
        }

        [Test]
        public void JoinTests_LeftJoin_Simple()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = from beer in bucket.Queryable<Beer>()
                        join breweryGroup in bucket.Queryable<Brewery>()
                            on beer.BreweryId equals N1Ql.Key(breweryGroup) into bg
                        from brewery in bg.DefaultIfEmpty()
                        select new {beer.Name, beer.Abv, BreweryName = brewery.Name};

                    foreach (var b in beers.Take(10))
                    {
                        Console.WriteLine("Beer {0} with ABV {1} is from {2}", b.Name, b.Abv, b.BreweryName);
                    }
                }
            }
        }

        [Test]
        public void JoinTests_LeftJoin_SortAndFilter()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = from beer in bucket.Queryable<Beer>()
                        join breweryGroup in bucket.Queryable<Brewery>()
                            on beer.BreweryId equals N1Ql.Key(breweryGroup) into bg
                        from brewery in bg.DefaultIfEmpty()
                        where beer.Abv > 4
                        orderby brewery.Name, beer.Name
                        select new {beer.Name, beer.Abv, BreweryName = brewery.Name};

                    foreach (var b in beers.Take(10))
                    {
                        Console.WriteLine("Beer {0} with ABV {1} is from {2}", b.Name, b.Abv, b.BreweryName);
                    }
                }
            }
        }

        [Test]
        public void JoinTests_LeftJoin_Prefiltered()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = from beer in bucket.Queryable<Beer>().Where(p => p.Type == "beer")
                        join breweryGroup in bucket.Queryable<Brewery>().Where(p => p.Type == "brewery")
                            on beer.BreweryId equals N1Ql.Key(breweryGroup) into bg
                        from brewery in bg.DefaultIfEmpty()
                        where beer.Abv > 4
                        orderby brewery.Name, beer.Name
                        select new {beer.Name, beer.Abv, BreweryName = brewery.Name};

                    foreach (var b in beers.Take(10))
                    {
                        Console.WriteLine("Beer {0} with ABV {1} is from {2}", b.Name, b.Abv, b.BreweryName);
                    }
                }
            }
        }

        [Test]
        public void NestTests_Unnest_Simple()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var breweries = from brewery in bucket.Queryable<Brewery>()
                        from address in brewery.Address
                        select new {name = brewery.Name, address};

                    foreach (var b in breweries.Take(10))
                    {
                        Console.WriteLine("Brewery {0} has address line {1}", b.name, b.address);
                    }
                }
            }
        }

        [Test]
        public void NestTests_Unnest_Sort()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var breweries = from brewery in bucket.Queryable<Brewery>()
                        from address in brewery.Address
                        orderby address
                        select new {name = brewery.Name, address};

                    foreach (var b in breweries.Take(10))
                    {
                        Console.WriteLine("Brewery {0} has address line {1}", b.name, b.address);
                    }
                }
            }
        }

        [Test()]
        public void SubqueryTests_ArraySubqueryWithFilter()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var breweries = from brewery in bucket.Queryable<Brewery>()
                        where brewery.Type == "brewery"
                        orderby brewery.Name
                        select new {name = brewery.Name, addresses = brewery.Address.Where(p => p.Length > 3)};

                    foreach (var b in breweries.Take(10))
                    {
                        Console.WriteLine("Brewery {0} has address {1}", b.name, string.Join(", ", b.addresses));
                    }
                }
            }
        }

        [Test()]
        public void SubqueryTests_ArraySubquerySelectNewObject()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var breweries = from brewery in bucket.Queryable<Brewery>()
                        where brewery.Type == "brewery"
                        orderby brewery.Name
                        select new {name = brewery.Name, addresses = brewery.Address.Select(p => new {address = p})};

                    foreach (var b in breweries.Take(10))
                    {
                        Console.WriteLine("Brewery {0} has address {1}", b.name,
                            string.Join(", ", b.addresses.Select(p => p.address)));
                    }
                }
            }
        }

        [Test()]
        public void SubqueryTests_ArraySubquerySorted()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var breweries = from brewery in bucket.Queryable<Brewery>()
                        where brewery.Type == "brewery"
                        orderby brewery.Name
                        select
                            new {name = brewery.Name, addresses = brewery.Address.OrderByDescending(p => p).ToArray()};

                    foreach (var b in breweries.Take(10))
                    {
                        Console.WriteLine("Brewery {0} has address {1}", b.name, string.Join(", ", b.addresses));
                    }
                }
            }
        }

        [Test()]
        public void AggregateTests_SimpleAverage()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var avg = bucket.Queryable<Beer>().Where(p => p.Type == "beer" && N1Ql.IsValued(p.Abv)).Average(p => p.Abv);

                    Console.WriteLine("Average ABV of all beers is {0}", avg);
                }
            }
        }

        [Test()]
        public void AggregateTests_SimpleCount()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var count = bucket.Queryable<Beer>().Count(p => p.Type == "beer");

                    Console.WriteLine("Number of beers is {0}", count);
                }
            }
        }

        [Test()]
        public void AggregateTests_GroupBy()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var breweries =
                        from beer in bucket.Queryable<Beer>()
                        where beer.Type == "beer"
                        group beer by beer.BreweryId
                        into g
                        orderby g.Key
                        select new { breweryid = g.Key, count = g.Count(), avgAbv = g.Average(p => p.Abv)};

                    foreach (var brewery in breweries)
                    {
                        Console.WriteLine("Brewery {0} has {1} beers with {2:f2} average ABV", brewery.breweryid, brewery.count, brewery.avgAbv);
                    }
                }
            }
        }

        [Test()]
        public void AggregateTests_Having()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var breweries =
                        from beer in bucket.Queryable<Beer>()
                        where beer.Type == "beer"
                        group beer by beer.BreweryId
                        into g
                        where g.Count() >= 5
                        orderby g.Key
                        select new { breweryid = g.Key, count = g.Count(), avgAbv = g.Average(p => p.Abv) };

                    foreach (var brewery in breweries)
                    {
                        Console.WriteLine("Brewery {0} has {1} beers with {2:f2} average ABV", brewery.breweryid, brewery.count, brewery.avgAbv);
                    }
                }
            }
        }

        [Test()]
        public void AggregateTests_OrderByAggregate()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var breweries =
                        from beer in bucket.Queryable<Beer>()
                        where beer.Type == "beer"
                        group beer by beer.BreweryId
                        into g
                        orderby g.Count() descending 
                        select new { breweryid = g.Key, count = g.Count(), avgAbv = g.Average(p => p.Abv) };

                    foreach (var brewery in breweries)
                    {
                        Console.WriteLine("Brewery {0} has {1} beers with {2:f2} average ABV", brewery.breweryid, brewery.count, brewery.avgAbv);
                    }
                }
            }
        }

        [Test()]
        public void AggregateTests_JoinBeforeGroupByAndMultipartKey()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var breweries =
                        from beer in bucket.Queryable<Beer>()
                        join brewery in bucket.Queryable<Brewery>() on beer.BreweryId equals N1Ql.Key(brewery)
                        where beer.Type == "beer"
                        group beer by new { breweryid = beer.BreweryId, breweryName = brewery.Name }
                        into g
                        select new { g.Key.breweryName, count = g.Count(), avgAbv = g.Average(p => p.Abv) };

                    foreach (var brewery in breweries)
                    {
                        Console.WriteLine("Brewery {0} has {1} beers with {2:f2} average ABV", brewery.breweryName, brewery.count, brewery.avgAbv);
                    }
                }
            }
        }

        [Test()]
        public void First_Empty()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = from beer in bucket.Queryable<Beer>()
                        where beer.Type == "abcdefg"
                        select new { beer.Name };

                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        // ReSharper disable once UnusedVariable
                        var temp = beers.First();
                    });
                }
            }
        }

        [Test()]
        public void First_HasResult()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = from beer in bucket.Queryable<Beer>()
                                where beer.Type == "beer"
                                select new { beer.Name };

                    Console.WriteLine(beers.First().Name);
                }
            }
        }

        [Test()]
        public void FirstOrDefault_Empty()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = from beer in bucket.Queryable<Beer>()
                                where beer.Type == "abcdefg"
                                select new { beer.Name };

                    var aBeer = beers.FirstOrDefault();
                    Assert.IsNull(aBeer);
                }
            }
        }

        [Test()]
        public void FirstOrDefault_HasResult()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = from beer in bucket.Queryable<Beer>()
                                where beer.Type == "beer"
                                select new { beer.Name };

                    var aBeer = beers.FirstOrDefault();
                    Assert.IsNotNull(aBeer);
                    Console.WriteLine(aBeer.Name);
                }
            }
        }

        [Test()]
        public void Single_Empty()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = from beer in bucket.Queryable<Beer>()
                                where beer.Type == "abcdefg"
                                select new { beer.Name };

                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        // ReSharper disable once UnusedVariable
                        var temp = beers.Single();
                    });
                }
            }
        }

        [Test()]
        public void Single_HasResult()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = from beer in bucket.Queryable<Beer>()
                                where beer.Name == "21A IPA"
                                select new { beer.Name };

                    Console.WriteLine(beers.Single().Name);
                }
            }
        }

        [Test()]
        public void Single_HasManyResults()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = from beer in bucket.Queryable<Beer>()
                                where beer.Type == "beer"
                                select new { beer.Name };

                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        // ReSharper disable once UnusedVariable
                        var temp = beers.Single();
                    });
                }
            }
        }

        [Test()]
        public void SingleOrDefault_Empty()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = from beer in bucket.Queryable<Beer>()
                                where beer.Type == "abcdefg"
                                select new { beer.Name };

                    var aBeer = beers.SingleOrDefault();
                    Assert.IsNull(aBeer);
                }
            }
        }

        [Test()]
        public void SingleOrDefault_HasResult()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = from beer in bucket.Queryable<Beer>()
                                where beer.Name == "21A IPA"
                                select new { beer.Name };

                    var aBeer = beers.SingleOrDefault();
                    Assert.IsNotNull(aBeer);
                    Console.WriteLine(aBeer.Name);
                }
            }
        }

        [Test()]
        public void SingleOrDefault_HasManyResults()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = from beer in bucket.Queryable<Beer>()
                                where beer.Type == "beer"
                                select new { beer.Name };

                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        // ReSharper disable once UnusedVariable
                        var temp = beers.SingleOrDefault();
                    });
                }
            }
        }
    }
}