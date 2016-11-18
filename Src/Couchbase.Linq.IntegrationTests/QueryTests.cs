using System;
using System.Collections.Generic;
using System.Linq;
using Couchbase.Configuration.Client;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Filters;
using Couchbase.Linq.IntegrationTests.Documents;
using Couchbase.Linq.Versioning;
using Couchbase.N1QL;
using NUnit.Framework;

namespace Couchbase.Linq.IntegrationTests
{
    [TestFixture]
    public class QueryTests : N1QlTestBase
    {
        [SetUp]
        public void TestSetUp()
        {
            Filters.DocumentFilterManager.Clear();
        }

        [Test]
        public void Map2PocoTests()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from b in context.Query<Beer>()
                select b;

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var beer in results)
            {
                Console.WriteLine(beer.Name);
            }
        }

        [Test]
        public void Map2PocoTests_Simple_Projections()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from b in context.Query<Beer>()
                select new {name = b.Name, abv = b.Abv};

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("{0} has {1} ABV", b.name, b.abv);
            }
        }

        [Test]
        public void Map2PocoTests_StronglyTyped_Projections()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from b in context.Query<Beer>()
                select new Beer {Name = b.Name, Abv = b.Abv};

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("{0} has {1} ABV", b.Name, b.Abv);
            }
        }

        [Test]
        public void Map2PocoTests_Simple_Projections_Where()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from b in context.Query<Beer>()
                where b.Type == "beer"
                select new {name = b.Name, abv = b.Abv};

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("{0} has {1} ABV", b.name, b.abv);
            }
        }

        [Test]
        public void Map2PocoTests_Simple_Projections_WhereNot()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from b in context.Query<Beer>()
                where b.Type == "beer" && !(b.Abv < 4)
                select new {name = b.Name, abv = b.Abv};

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count);

            foreach (var b in results)
            {
                Console.WriteLine("{0} has {1} ABV", b.name, b.abv);
            }
        }

        [Test]
        public void Map2PocoTests_Simple_Projections_WhereDateTime()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from b in context.Query<Beer>()
                where (b.Type == "beer") && (b.Updated >= new DateTime(2010, 1, 1))
                select new {name = b.Name, updated = b.Updated};

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count);

            foreach (var b in results)
            {
                Console.WriteLine("{0} last updated {1:g}", b.name, b.updated);
            }
        }

        [Test]
        public void Map2PocoTests_Simple_Projections_WhereEnum()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = (from b in context.Query<BeerWithEnum>()
                where (b.Type == "beer") && (b.Style == BeerStyle.OatmealStout)
                select new {name = b.Name, style = b.Style})
                .Take(1).ToList();

            Assert.IsNotEmpty(beers);

            foreach (var b in beers)
            {
                Assert.AreEqual(BeerStyle.OatmealStout, b.style);

                Console.WriteLine("{0} has style {1}", b.name, b.style);
            }
        }

        [Test]
        public void Map2PocoTests_Simple_Projections_StartsWith()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from b in context.Query<Beer>()
                        where b.Type == "beer" && b.Name.StartsWith("563")
                        select new { name = b.Name, abv = b.Abv };

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count);

            foreach (var b in results)
            {
                Console.WriteLine("{0} has {1} ABV", b.name, b.abv);
            }
        }

        [Test]
        public void Map2PocoTests_Simple_Projections_EndsWithExpression()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            // This query is not useful, but tests more advanced string contains use cases
            var beers = from b in context.Query<Beer>()
                        where b.Type == "beer" && b.Name.EndsWith(b.Name.Substring(b.Name.Length - 3))
                        select new { name = b.Name, abv = b.Abv };

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count);

            foreach (var b in results)
            {
                Console.WriteLine("{0} has {1} ABV", b.name, b.abv);
            }
        }

        [Test]
        public void Map2PocoTests_Simple_Projections_Limit()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = (from b in context.Query<Beer>()
                where b.Type == "beer"
                select new {name = b.Name, abv = b.Abv}).
                Take(1).
                Skip(5);

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("{0} has {1} ABV", b.name, b.abv);
            }
        }

        [Test]
        public void Map2PocoTests_Simple_Projections_Meta()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = (from b in context.Query<Beer>()
                where b.Type == "beer"
                select new {name = b.Name, meta = N1QlFunctions.Meta(b)}).
                Take(1);

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("{0} has metadata {1}", b.name, b.meta);
            }
        }

        [Test]
        public void Map2PocoTests_Simple_Projections_Key()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = (from b in context.Query<Beer>()
                         where b.Type == "beer"
                         select new { name = b.Name, key = N1QlFunctions.Key(b) }).
                Take(1);

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Assert.NotNull(b.key);
                Console.WriteLine("{0} has key {1}", b.name, b.key);
            }
        }

        [Test]
        public void Map2PocoTests_Explain()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var explanation = (from b in context.Query<Beer>()
                where b.Type == "beer"
                select b).
                Explain();

            Console.WriteLine(explanation);
        }

        [Test]
        public void Map2PocoTests_Explain_QueryWithPropertyExtraction()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var explanation = (from b in context.Query<Beer>()
                where b.Type == "beer"
                select b.Abv).
                Explain();

            Console.WriteLine(explanation);
        }

        [Test]
        public void Map2PocoTests_NewObjectsInArray()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var query = from brewery in context.Query<Brewery>()
                where brewery.Type == "brewery"
                select
                    new
                    {
                        name = brewery.Name,
                        list =
                            new[]
                            {new {part = brewery.City}, new {part = brewery.State}, new {part = brewery.Code}}
                    };

            var results = query.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var brewery in results)
            {
                Console.WriteLine("Brewery {0} has address parts {1}", brewery.name,
                    string.Join(", ", brewery.list.Select(p => p.part)));
            }
        }

        [Test]
        public void NoProjection_Meta()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = (from b in context.Query<Beer>()
                where b.Type == "beer"
                select N1QlFunctions.Meta(b)).
                Take(1);

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine(b);
            }
        }

        [Test]
        public void NoProjection_Number()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = (from b in context.Query<Beer>()
                where b.Type == "beer"
                select b.Abv).
                Take(1);

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine(b);
            }
        }

        [Test]
        public void UseKeys_SelectDocuments()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var query =
                from brewery in
                    context.Query<Brewery>().UseKeys(new[] {"21st_amendment_brewery_cafe", "357"})
                select new {name = brewery.Name};

            var results = query.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var brewery in results)
            {
                Console.WriteLine("Brewery {0}", brewery.name);
            }
        }

        [Test]
        public void UseIndex_SelectDocuments()
        {
            // This test simply confirms that the USE INDEX clause generated is valid N1QL.  There's not much point
            // in the actual USE INDEX clause itself in this query, since the index isn't used in the predicate.
            // In a real world query, this should be a specific index helpful to the query.

            var bucket = ClusterHelper.GetBucket("beer-sample");

            EnsureIndexExists(bucket, "brewery_id", "brewery_id");

            var context = new BucketContext(bucket);

            var query =
                from brewery in
                    context.Query<Brewery>().UseIndex("brewery_id")
                select new { name = brewery.Name };

            var results = query.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var brewery in results)
            {
                Console.WriteLine("Brewery {0}", brewery.name);
            }
        }

        [Test]
        public void AnyAllTests_AnyNestedArrayWithFilter()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var breweries = (from b in context.Query<Brewery>()
                where b.Type == "brewery" && b.Address.Any(p => p == "563 Second Street")
                select new {name = b.Name, address = b.Address}).
                Take(1).
                ToList();

            Assert.IsNotEmpty(breweries);
            Assert.True(breweries.All(p => p.address.Contains("563 Second Street")));
        }

        [Test, Ignore("Test fails with primary scan")]
        public void AnyAllTests_AnyNestedArrayWithFilter_UsesArrayIndex()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");

            var clusterVersion = VersionProvider.Current.GetVersion(bucket);
            if (clusterVersion < FeatureVersions.ArrayIndexes)
            {
                Assert.Ignore("Cluster does not support array indexes, test skipped.");
            }

            // This test requires the following index:
            //   CREATE INDEX brewery_address ON `beer-sample` (DISTINCT ARRAY x FOR x IN address END) WHERE type = 'brewery'

            // It can't be automatically created currently because the bucket manager
            // doesn't support creating array or function-based indexes, only plain attribute indexes

            var context = new BucketContext(bucket);

            var explanation = (from b in context.Query<Brewery>()
                               where b.Type == "brewery" && b.Address.AsQueryable("x").Any(p => p == "563 Second Street")
                               select new { name = b.Name, address = b.Address }).
                Explain();

            Assert.AreEqual("DistinctScan", explanation.plan["~children"][0]["#operator"].ToString());
            Assert.AreEqual("IndexScan", explanation.plan["~children"][0].scan["#operator"].ToString());
            Assert.AreEqual("brewery_address", explanation.plan["~children"][0].scan.index.ToString());
        }

        [Test]
        public void AnyAllTests_AnyOnMainDocument_ReturnsTrue()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var hasBreweries = (from b in context.Query<Brewery>()
                where b.Type == "brewery"
                select new {name = b.Name, address = b.Address}).Take(1).
                Any();

            Assert.True(hasBreweries);
        }

        [Test]
        public void AnyAllTests_AnyOnMainDocument_ReturnsFalse()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var hasFaketype = (from b in context.Query<Brewery>()
                where b.Type == "faketype"
                select new {name = b.Name, address = b.Address}).
                Take(1).
                Any();

            Assert.False(hasFaketype);
        }

        [Test]
        public void AnyAllTests_AllNestedArray()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var breweries = (from b in context.Query<Brewery>()
                where b.Type == "brewery" && b.Address.All(p => p == "563 Second Street")
                select new {name = b.Name, address = b.Address}).
                Take(1).
                ToList();

            Assert.IsNotEmpty(breweries);
            Assert.True(breweries.SelectMany(p => p.address).All(p => p == "563 Second Street"));
        }

        [Test]
        public void AnyAllTests_AllNestedArrayPrefiltered()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            // Note: This query isn't very useful in the real world
            // However, it does demonstrate how to prefilter the collection before all is run
            // Which is behaviorly different then adding the Where predicate inside the All predicate
            // In this example, all breweries which have NO address 563 Second Street will be returned

            var breweries = (from b in context.Query<Brewery>()
                where
                    b.Type == "brewery" &&
                    b.Address.Where(p => p == "563 Second Street").All(p => p == "101 Fake Street")
                orderby b.Name
                select new {name = b.Name, address = b.Address}).
                Take(1).
                ToList();

            Assert.IsNotEmpty(breweries);
            Assert.False(breweries.SelectMany(p => p.address).Any(p => p == "563 Second Street"));
        }

        [Test]
        public void AnyAllTests_AllOnMainDocument_ReturnsFalse()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var isAllBreweries = context.Query<Brewery>().All(p => p.Type == "brewery");

            Assert.False(isAllBreweries);
        }

        [Test]
        public void AnyAllTests_AllOnMainDocument_ReturnsTrue()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var allBreweriesHaveAddress = (from b in context.Query<Brewery>()
                where b.Type == "brewery"
                select new {b.Name})
                .All(p => p.Name != "");

            Assert.True(allBreweriesHaveAddress);
        }

        [Test]
        public void Map2PocoTests_Simple_Projections_TypeFilterAttribute()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = (from b in context.Query<BeerFiltered>()
                select new {type = b.Type}).
                AsEnumerable();

            Assert.True(beers.All(p => p.type == "beer"));
        }

        [Test]
        public void Map2PocoTests_Simple_Projections_TypeFilterRuntime()
        {
            DocumentFilterManager.SetFilter(new BreweryFilter());

            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var breweries = (from b in context.Query<Brewery>()
                select new {type = b.Type})
                .AsEnumerable();

            Assert.True(breweries.All(p => p.type == "brewery"));
        }

        public void Map2PocoTests_Simple_Projections_MetaWhere()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = (from b in context.Query<Beer>()
                where b.Type == "beer" && N1QlFunctions.Meta(b).Type == "json"
                select new {name = b.Name}).
                Take(1);

            var results = beers.Take(1);
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("{0} is a JSON document", b.name);
            }
        }

        public void Map2PocoTests_Simple_Projections_MetaId()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = (from b in context.Query<Beer>()
                where b.Type == "beer"
                select new {name = b.Name, id = N1QlFunctions.Meta(b).Id}).
                Take(1);

            var results = beers.Take(1);
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("{0} has id {1}", b.name, b.id);
            }
        }

        public void AnyAllTests_AnyNestedArray()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var breweries = (from b in context.Query<Brewery>()
                where b.Type == "brewery" && b.Address.Any()
                select new {name = b.Name, address = b.Address}).
                ToList();

            Assert.IsNotEmpty(breweries);
            Assert.True(breweries.All(p => p.address.Any()));
        }

        [Test]
        public void JoinTests_InnerJoin_Simple()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from beer in context.Query<Beer>()
                join brewery in context.Query<Brewery>()
                    on beer.BreweryId equals N1QlFunctions.Key(brewery)
                select new {beer.Name, beer.Abv, BreweryName = brewery.Name};

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("Beer {0} with ABV {1} is from {2}", b.Name, b.Abv, b.BreweryName);
            }
        }

        [Test]
        public void JoinTests_InnerJoin_SortAndFilter()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from beer in context.Query<Beer>()
                join brewery in context.Query<Brewery>()
                    on beer.BreweryId equals N1QlFunctions.Key(brewery)
                where brewery.Geo.Longitude > -80
                orderby beer.Name
                select new {beer.Name, beer.Abv, BreweryName = brewery.Name};

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("Beer {0} with ABV {1} is from {2}", b.Name, b.Abv, b.BreweryName);
            }
        }

        [Test]
        public void JoinTests_InnerJoin_Prefiltered()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from beer in context.Query<Beer>().Where(p => p.Type == "beer")
                join brewery in context.Query<Brewery>().Where(p => p.Type == "brewery")
                    on beer.BreweryId equals N1QlFunctions.Key(brewery)
                where brewery.Geo.Longitude > -80
                orderby beer.Name
                select new {beer.Name, beer.Abv, BreweryName = brewery.Name};

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("Beer {0} with ABV {1} is from {2}", b.Name, b.Abv, b.BreweryName);
            }
        }

        [Test]
        public void JoinTests_InnerJoin_IndexJoin()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");

            EnsureIndexExists(bucket, "brewery_id", "brewery_id");

            var clusterVersion = VersionProvider.Current.GetVersion(bucket);
            if (clusterVersion < FeatureVersions.IndexJoin)
            {
                Assert.Ignore("Cluster does not support index joins, test skipped.");
            }

            var context = new BucketContext(bucket);

            var beers = from brewery in context.Query<Brewery>()
                        join beer in context.Query<Beer>()
                            on N1QlFunctions.Key(brewery) equals beer.BreweryId
                        where (beer.Type == "beer") && (brewery.Type == "brewery")
                        select new { beer.Name, beer.Abv, BreweryName = brewery.Name };

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("Beer {0} with ABV {1} is from {2}", b.Name, b.Abv, b.BreweryName);
            }
        }

        [Test]
        public void JoinTests_LeftJoin_Simple()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from beer in context.Query<Beer>()
                join breweryGroup in context.Query<Brewery>()
                    on beer.BreweryId equals N1QlFunctions.Key(breweryGroup) into bg
                from brewery in bg.DefaultIfEmpty()
                select new {beer.Name, beer.Abv, BreweryName = brewery.Name};

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("Beer {0} with ABV {1} is from {2}", b.Name, b.Abv, b.BreweryName);
            }
        }

        [Test]
        public void JoinTests_LeftJoin_SortAndFilter()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from beer in context.Query<Beer>()
                join breweryGroup in context.Query<Brewery>()
                    on beer.BreweryId equals N1QlFunctions.Key(breweryGroup) into bg
                from brewery in bg.DefaultIfEmpty()
                where beer.Abv > 4
                orderby brewery.Name, beer.Name
                select new {beer.Name, beer.Abv, BreweryName = brewery.Name};

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("Beer {0} with ABV {1} is from {2}", b.Name, b.Abv, b.BreweryName);
            }
        }

        [Test]
        public void JoinTests_LeftJoin_Prefiltered()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from beer in context.Query<Beer>().Where(p => p.Type == "beer")
                join breweryGroup in context.Query<Brewery>().Where(p => p.Type == "brewery")
                    on beer.BreweryId equals N1QlFunctions.Key(breweryGroup) into bg
                from brewery in bg.DefaultIfEmpty()
                where beer.Abv > 4
                orderby brewery.Name, beer.Name
                select new {beer.Name, beer.Abv, BreweryName = brewery.Name};

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("Beer {0} with ABV {1} is from {2}", b.Name, b.Abv, b.BreweryName);
            }
        }

        [Test]
        public void JoinTests_LeftJoin_IndexJoin()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");

            EnsureIndexExists(bucket, "brewery_id", "brewery_id");

            var clusterVersion = VersionProvider.Current.GetVersion(bucket);
            if (clusterVersion < FeatureVersions.IndexJoin)
            {
                Assert.Ignore("Cluster does not support index joins, test skipped.");
            }

            var context = new BucketContext(bucket);

            var beers = from brewery in context.Query<Brewery>()
                        join beer in context.Query<Beer>()
                            on N1QlFunctions.Key(brewery) equals beer.BreweryId into bg
                        from beer in bg.DefaultIfEmpty()
                        where (beer.Type == "beer") && (brewery.Type == "brewery")
                        select new { beer.Name, beer.Abv, BreweryName = brewery.Name };

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("Beer {0} with ABV {1} is from {2}", b.Name, b.Abv, b.BreweryName);
            }
        }

        [Test]
        public void NestTests_Unnest_Simple()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var breweries = from brewery in context.Query<Brewery>()
                from address in brewery.Address
                select new {name = brewery.Name, address};

            var results = breweries.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("Brewery {0} has address line {1}", b.name, b.address);
            }
        }

        [Test]
        public void NestTests_Unnest_Sort()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var breweries = from brewery in context.Query<Brewery>()
                from address in brewery.Address
                orderby address
                select new {name = brewery.Name, address};

            var results = breweries.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("Brewery {0} has address line {1}", b.name, b.address);
            }
        }

        [Test]
        public void NestTests_Nest_IndexJoin()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");

            EnsureIndexExists(bucket, "brewery_id", "brewery_id");

            var clusterVersion = VersionProvider.Current.GetVersion(bucket);
            if (clusterVersion < FeatureVersions.IndexJoin)
            {
                Assert.Ignore("Cluster does not support index joins, test skipped.");
            }

            var context = new BucketContext(bucket);

            var breweries = from brewery in context.Query<Brewery>()
                join beer in context.Query<Beer>() on N1QlFunctions.Key(brewery) equals beer.BreweryId into beers
                where brewery.Type == "brewery"
                select new {name = brewery.Name, beers};

            var results = breweries.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var brewery in results)
            {
                foreach (var beer in brewery.beers)
                {
                    Console.WriteLine("Beer {0} with ABV {1} is from {2}", beer.Name, beer.Abv, brewery.name);
                }
            }
        }

        [Test]
        public void NestTests_Nest_IndexJoinPrefiltered()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");

            EnsureIndexExists(bucket, "brewery_id", "brewery_id");

            var clusterVersion = VersionProvider.Current.GetVersion(bucket);
            if (clusterVersion < FeatureVersions.IndexJoin)
            {
                Assert.Ignore("Cluster does not support index joins, test skipped.");
            }

            var context = new BucketContext(bucket);

            var breweries = from brewery in context.Query<Brewery>()
                            join beer in context.Query<BeerFiltered>() on N1QlFunctions.Key(brewery) equals beer.BreweryId into beers
                            where brewery.Type == "brewery"
                            select new { name = brewery.Name, beers };

            var results = breweries.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var brewery in results)
            {
                foreach (var beer in brewery.beers)
                {
                    Console.WriteLine("Beer {0} with ABV {1} is from {2}", beer.Name, beer.Abv, brewery.name);
                }
            }
        }

        [Test]
        public void SubqueryTests_ArraySubqueryWithFilter()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var breweries = from brewery in context.Query<Brewery>()
                where brewery.Type == "brewery"
                orderby brewery.Name
                select new {name = brewery.Name, addresses = brewery.Address.Where(p => p.Length > 3)};

            var results = breweries.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("Brewery {0} has address {1}", b.name, string.Join(", ", b.addresses));
            }
        }

        [Test]
        public void SubqueryTests_ArraySubqueryContains()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var breweries = from brewery in context.Query<Brewery>()
                where brewery.Type == "brewery" && brewery.Address.Contains("563 Second Street")
                orderby brewery.Name
                select new {name = brewery.Name, addresses = brewery.Address};

            var results = breweries.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("Brewery {0} has address {1}", b.name, string.Join(", ", b.addresses));
            }
        }

        [Test]
        public void SubqueryTests_StaticArraySubqueryContains()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var breweryNames = new[] { "21st Amendment Brewery Cafe", "357" };
            var breweries = from brewery in context.Query<Brewery>()
                            where brewery.Type == "brewery" && breweryNames.Contains(brewery.Name)
                            orderby brewery.Name
                            select new { name = brewery.Name, addresses = brewery.Address };

            var results = breweries.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("Brewery {0} has address {1}", b.name, string.Join(", ", b.addresses));
            }
        }

        [Test]
        public void SubqueryTests_ArraySubquerySelectNewObject()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var breweries = from brewery in context.Query<Brewery>()
                where brewery.Type == "brewery"
                orderby brewery.Name
                select new {name = brewery.Name, addresses = brewery.Address.Select(p => new {address = p})};

            var results = breweries.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("Brewery {0} has address {1}", b.name,
                    string.Join(", ", b.addresses.Select(p => p.address)));
            }
        }

        [Test]
        public void SubqueryTests_ArraySubquerySorted()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var breweries = from brewery in context.Query<Brewery>()
                where brewery.Type == "brewery"
                orderby brewery.Name
                select
                    new {name = brewery.Name, addresses = brewery.Address.OrderByDescending(p => p).ToArray()};

            var results = breweries.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("Brewery {0} has address {1}", b.name, string.Join(", ", b.addresses));
            }
        }

        [Test]
        public void SubqueryTests_Union()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var names = (from brewery in context.Query<Brewery>()
                where brewery.Type == "brewery"
                select new { AnyName = brewery.Name })
                .Union(from beer in context.Query<Beer>()
                    where beer.Type == "beer"
                    select new { AnyName = beer.Name })
                .OrderBy(p => p.AnyName);

            var results = names.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine(b.AnyName);
            }
        }

        [Test]
        public void SubqueryTests_UnionAll()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var names = (from brewery in context.Query<Brewery>()
                         where brewery.Type == "brewery"
                         select new { AnyName = brewery.Name })
                .Concat(from beer in context.Query<Beer>()
                        where beer.Type == "beer"
                        select new { AnyName = beer.Name })
                .OrderBy(p => p.AnyName);

            var results = names.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine(b.AnyName);
            }
        }

        [Test]
        public void AggregateTests_SimpleAverage()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var avg =
                context.Query<Beer>().Where(p => p.Type == "beer" && N1QlFunctions.IsValued(p.Abv)).Average(p => p.Abv);
            Assert.Greater(avg, 0);
            Console.WriteLine("Average ABV of all beers is {0}", avg);
        }

        [Test]
        public void AggregateTests_SimpleCount()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var count = context.Query<Beer>().Count(p => p.Type == "beer");
            Assert.Greater(count, 0);
            Console.WriteLine("Number of beers is {0}", count);
        }

        [Test]
        public void AggregateTests_GroupBy()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var breweries =
                from beer in context.Query<Beer>()
                where beer.Type == "beer"
                group beer by beer.BreweryId
                into g
                orderby g.Key
                select new {breweryid = g.Key, count = g.Count(), avgAbv = g.Average(p => p.Abv)};

            var results = breweries.Take(1).ToList();
            Assert.AreEqual(1, results.Count);

            foreach (var brewery in results)
            {
                Console.WriteLine("Brewery {0} has {1} beers with {2:f2} average ABV", brewery.breweryid, brewery.count,
                    brewery.avgAbv);
            }
        }

        [Test]
        public void AggregateTests_Having()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var breweries =
                from beer in context.Query<Beer>()
                where beer.Type == "beer"
                group beer by beer.BreweryId
                into g
                where g.Count() >= 5
                orderby g.Key
                select new {breweryid = g.Key, count = g.Count(), avgAbv = g.Average(p => p.Abv)};

            var results = breweries.Take(1).ToList();
            Assert.AreEqual(1, results.Count);

            foreach (var brewery in results)
            {
                Console.WriteLine("Brewery {0} has {1} beers with {2:f2} average ABV", brewery.breweryid, brewery.count,
                    brewery.avgAbv);
            }
        }

        [Test]
        public void AggregateTests_OrderByAggregate()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var breweries =
                from beer in context.Query<Beer>()
                where beer.Type == "beer"
                group beer by beer.BreweryId
                into g
                orderby g.Count() descending
                select new {breweryid = g.Key, count = g.Count(), avgAbv = g.Average(p => p.Abv)};

            var results = breweries.Take(1).ToList();
            Assert.AreEqual(1, results.Count);

            foreach (var brewery in results)
            {
                Console.WriteLine("Brewery {0} has {1} beers with {2:f2} average ABV", brewery.breweryid, brewery.count,
                    brewery.avgAbv);
            }
        }

        [Test]
        public void AggregateTests_JoinBeforeGroupByAndMultipartKey()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var breweries =
                from beer in context.Query<Beer>()
                join brewery in context.Query<Brewery>() on beer.BreweryId equals N1QlFunctions.Key(brewery)
                where beer.Type == "beer"
                group beer by new {breweryid = beer.BreweryId, breweryName = brewery.Name}
                into g
                select new {g.Key.breweryName, count = g.Count(), avgAbv = g.Average(p => p.Abv)};

            var results = breweries.Take(1).ToList();
            Assert.AreEqual(1, results.Count);

            foreach (var brewery in results)
            {
                Console.WriteLine("Brewery {0} has {1} beers with {2:f2} average ABV", brewery.breweryName,
                    brewery.count, brewery.avgAbv);
            }
        }

        [Test]
        public void First_Empty()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "abcdefg"
                select new {beer.Name};

            Assert.Throws<InvalidOperationException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var temp = beers.First();
            });
        }

        [Test]
        public void First_HasResult()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name};

            Console.WriteLine(beers.First().Name);
        }

        [Test]
        public void FirstOrDefault_Empty()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "abcdefg"
                select new {beer.Name};

            var aBeer = beers.FirstOrDefault();
            Assert.IsNull(aBeer);
        }

        [Test]
        public void FirstOrDefault_HasResult()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name};

            var aBeer = beers.FirstOrDefault();
            Assert.IsNotNull(aBeer);
            Console.WriteLine(aBeer.Name);
        }

        [Test]
        public void Single_Empty()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "abcdefg"
                select new {beer.Name};

            Assert.Throws<InvalidOperationException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var temp = beers.Single();
            });
        }

        [Test]
        public void Single_HasResult()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from beer in context.Query<Beer>()
                where beer.Name == "21A IPA"
                select new {beer.Name};

            Console.WriteLine(beers.Single().Name);
        }

        [Test]
        public void Single_HasManyResults()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name};

            Assert.Throws<InvalidOperationException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var temp = beers.Single();
            });
        }

        [Test]
        public void SingleOrDefault_Empty()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "abcdefg"
                select new {beer.Name};

            var aBeer = beers.SingleOrDefault();
            Assert.IsNull(aBeer);
        }

        [Test]
        public void SingleOrDefault_HasResult()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from beer in context.Query<Beer>()
                where beer.Name == "21A IPA"
                select new {beer.Name};

            var aBeer = beers.SingleOrDefault();
            Assert.IsNotNull(aBeer);
            Console.WriteLine(aBeer.Name);
        }

        [Test]
        public void SingleOrDefault_HasManyResults()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name};

            Assert.Throws<InvalidOperationException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var temp = beers.SingleOrDefault();
            });
        }

        #region "Date/time functions"

        [Test]
        public void DateTime_DateAdd()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name, Updated = N1QlFunctions.DateAdd(beer.Updated, -10, N1QlDatePart.Day)};

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("Beer {0} was updated 10 days after {1:g}", b.Name, b.Updated);
            }
        }

        [Test]
        public void DateTime_DateDiff()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name, DaysOld = N1QlFunctions.DateDiff(DateTime.Now, beer.Updated, N1QlDatePart.Day)};

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("Beer {0} is {1} days old", b.Name, b.DaysOld);
            }
        }

        [Test]
        public void DateTime_DatePart()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name, Year = N1QlFunctions.DatePart(beer.Updated, N1QlDatePart.Year)};

            var results = beers.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine("Beer {0} was updated in {1:0000}", b.Name, b.Year);
            }
        }

        [Test]
        public void DateTime_DateTrunc()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name, Updated = N1QlFunctions.DateTrunc(beer.Updated, N1QlDatePart.Month)};

            foreach (var b in beers.Take(1))
            {
                Console.WriteLine("Beer {0} is in {1:MMMM yyyy}", b.Name, b.Updated);
            }
        }

        #endregion
    }
}