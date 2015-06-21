﻿using System;
using System.Collections.Generic;
using System.Linq;
using Couchbase.Configuration.Client;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Tests.Documents;
using Couchbase.N1QL;
using NUnit.Framework;

namespace Couchbase.Linq.Tests
{
    [TestFixture]
    public class BeerSampleTests : N1QLTestBase
    {
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
        public void AnyAllTests_AnyNestedArray()
        {
            using (var cluster = new Cluster())
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
        public void AnyAllTests_AnyNestedArrayWithFilter()
        {
            using (var cluster = new Cluster())
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var breweries = (from b in bucket.Queryable<Brewery>()
                                     where b.Type == "brewery" && b.Address.Any(p => p == "563 Second Street")
                                     select new { name = b.Name, address = b.Address }).
                        ToList();

                    Assert.IsNotEmpty(breweries);
                    Assert.True(breweries.All(p => p.address.Contains("563 Second Street")));
                }
            }
        }

        [Test]
        public void AnyAllTests_AnyOnMainDocument_ReturnsTrue()
        {
            using (var cluster = new Cluster())
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var hasBreweries = (from b in bucket.Queryable<Brewery>()
                                        where b.Type == "brewery"
                                        select new { name = b.Name, address = b.Address }).
                        Any();

                    Assert.True(hasBreweries);
                }
            }
        }

        [Test]
        public void AnyAllTests_AnyOnMainDocument_ReturnsFalse()
        {
            using (var cluster = new Cluster())
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var hasFaketype = (from b in bucket.Queryable<Brewery>()
                                       where b.Type == "faketype"
                                       select new { name = b.Name, address = b.Address }).
                        Any();

                    Assert.False(hasFaketype);
                }
            }
        }

        [Test]
        public void AnyAllTests_AllNestedArray()
        {
            using (var cluster = new Cluster())
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var breweries = (from b in bucket.Queryable<Brewery>()
                                     where b.Type == "brewery" && b.Address.All(p => p == "563 Second Street")
                                     select new { name = b.Name, address = b.Address }).
                        ToList();

                    Assert.IsNotEmpty(breweries);
                    Assert.True(breweries.SelectMany(p => p.address).All(p => p == "563 Second Street"));
                }
            }
        }

        [Test]
        public void AnyAllTests_AllNestedArrayPrefiltered()
        {
            using (var cluster = new Cluster())
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    // Note: This query isn't very useful in the real world
                    // However, it does demonstrate how to prefilter the collection before all is run
                    // Which is behaviorly different then adding the Where predicate inside the All predicate
                    // In this example, all breweries which have NO address 563 Second Street will be returned

                    var breweries = (from b in bucket.Queryable<Brewery>()
                                     where b.Type == "brewery" && b.Address.Where(p => p == "563 Second Street").All(p => p == "101 Fake Street")
                                     orderby b.Name
                                     select new { name = b.Name, address = b.Address }).
                        ToList();

                    Assert.IsNotEmpty(breweries);
                    Assert.False(breweries.SelectMany(p => p.address).Any(p => p == "563 Second Street"));
                }
            }
        }

        [Test]
        public void AnyAllTests_AllOnMainDocument_ReturnsFalse()
        {
            using (var cluster = new Cluster())
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
            using (var cluster = new Cluster())
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var allBreweriesHaveAddress = (from b in bucket.Queryable<Brewery>()
                                                   where b.Type == "brewery"
                                                   select new { b.Name })
                        .All(p => p.Name != "");

                    Assert.True(allBreweriesHaveAddress);
                }
            }
        }

    }
}