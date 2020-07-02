using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.Core.Version;
using Couchbase.KeyValue;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Filters;
using Couchbase.Linq.IntegrationTests.Documents;
using Couchbase.Linq.Utils;
using Couchbase.Linq.Versioning;
using NUnit.Framework;

namespace Couchbase.Linq.IntegrationTests
{
    [TestFixture]
    public class QueryTests : N1QlTestBase
    {
        private IBucket _travelSample;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            await PrepareBeerDocuments();

            _travelSample = await TestSetup.Cluster.BucketAsync("travel-sample");
        }

        [SetUp]
        public void TestSetUp()
        {
            Filters.DocumentFilterManager.Clear();
        }

        [Test]
        public void Map2PocoTests()
        {
            var context = new BucketContext(TestSetup.Bucket);

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
        public async Task Map2PocoTestsAsync()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var beers = from b in context.Query<Beer>()
                select b;

            var results = await beers.Take(1).ToListAsync();
            Assert.AreEqual(1, results.Count());

            foreach (var beer in results)
            {
                Console.WriteLine(beer.Name);
            }
        }

        [Test]
        public void Map2PocoTests_Simple_Projections()
        {
            var context = new BucketContext(TestSetup.Bucket);

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
            var context = new BucketContext(TestSetup.Bucket);

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
            var context = new BucketContext(TestSetup.Bucket);

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
            var context = new BucketContext(TestSetup.Bucket);

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
            var context = new BucketContext(TestSetup.Bucket);

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
            var context = new BucketContext(TestSetup.Bucket);

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
            var context = new BucketContext(TestSetup.Bucket);

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
            var context = new BucketContext(TestSetup.Bucket);

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
            var context = new BucketContext(TestSetup.Bucket);

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
            var context = new BucketContext(TestSetup.Bucket);


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
            var context = new BucketContext(TestSetup.Bucket);


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
            var context = new BucketContext(TestSetup.Bucket);


            var explanation = (from b in context.Query<Beer>()
                where b.Type == "beer"
                select b).
                Explain();

            Console.WriteLine(explanation);
        }

        [Test]
        public async Task Map2PocoTests_ExplainAsync()
        {
            var context = new BucketContext(TestSetup.Bucket);


            var explanation = await (from b in context.Query<Beer>()
                    where b.Type == "beer"
                    select b).
                ExplainAsync();

            Console.WriteLine(explanation);
        }

        [Test]
        public void Map2PocoTests_Explain_QueryWithPropertyExtraction()
        {
            var context = new BucketContext(TestSetup.Bucket);


            var explanation = (from b in context.Query<Beer>()
                where b.Type == "beer"
                select b.Abv).
                Explain();

            Console.WriteLine(explanation);
        }

        [Test]
        public void Map2PocoTests_NewObjectsInArray()
        {
            var context = new BucketContext(TestSetup.Bucket);


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
            var context = new BucketContext(TestSetup.Bucket);


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
            var context = new BucketContext(TestSetup.Bucket);


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
            var context = new BucketContext(TestSetup.Bucket);


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
        public async Task UseIndex_SelectDocuments()
        {
            // This test simply confirms that the USE INDEX clause generated is valid N1QL.  There's not much point
            // in the actual USE INDEX clause itself in this query, since the index isn't used in the predicate.
            // In a real world query, this should be a specific index helpful to the query.

            var context = new BucketContext(TestSetup.Bucket);

            await EnsureIndexExists(context.Bucket, "brewery_id", "brewery_id");



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
        public async Task UseIndex_RightSideOfJoin_SelectDocuments()
        {
            // This test simply confirms that the USE INDEX clause generated is valid N1QL.  There's not much point
            // in the actual USE INDEX clause itself in this query, since the index isn't used in the predicate.
            // In a real world query, this should be a specific index helpful to the query.

            var context = new BucketContext(_travelSample);

            var versionProvider = TestSetup.Cluster.ClusterServices.GetRequiredService<IClusterVersionProvider>();
            var clusterVersion = await versionProvider.GetVersionAsync() ?? FeatureVersions.DefaultVersion;
            if (clusterVersion < FeatureVersions.AnsiJoin)
            {
                Assert.Ignore("Cluster does not support ANSI joins, test skipped.");
            }

            var query =
                from route in context.Query<Route>()
                join airport in context.Query<Airport>()
                        .UseIndex("def_faa")
                        .Where(p => p.Type == "airport")
                    on route.DestinationAirport equals airport.Faa
                where route.Type == "route"
                select new { airport.AirportName, route.Airline };

            var results = query.Take(1).ToList();
            Assert.AreEqual(1, results.Count);

            foreach (var b in results)
            {
                Console.WriteLine("Route for airline {0} goes to {1}", b.Airline, b.AirportName);
            }
        }

        [Test]
        public async Task UseHash_SelectDocuments()
        {
            var context = new BucketContext(_travelSample);

            var versionProvider = TestSetup.Cluster.ClusterServices.GetRequiredService<IClusterVersionProvider>();
            var clusterVersion = await versionProvider.GetVersionAsync() ?? FeatureVersions.DefaultVersion;
            if (clusterVersion < FeatureVersions.AnsiJoin)
            {
                Assert.Ignore("Cluster does not support ANSI joins, test skipped.");
            }



            var query =
                from route in context.Query<Route>()
                join airport in context.Query<Airport>()
                        .UseHash(HashHintType.Build)
                        .Where(p => p.Type == "airport")
                    on route.DestinationAirport equals airport.Faa
                where route.Type == "route"
                select new { airport.AirportName, route.Airline };

            var results = query.Take(1).ToList();
            Assert.AreEqual(1, results.Count);

            foreach (var b in results)
            {
                Console.WriteLine("Route for airline {0} goes to {1}", b.Airline, b.AirportName);
            }
        }

        [Test]
        public async Task UseHashAndIndex_SelectDocuments()
        {
            var context = new BucketContext(_travelSample);

            var versionProvider = TestSetup.Cluster.ClusterServices.GetRequiredService<IClusterVersionProvider>();
            var clusterVersion = await versionProvider.GetVersionAsync() ?? FeatureVersions.DefaultVersion;
            if (clusterVersion < FeatureVersions.AnsiJoin)
            {
                Assert.Ignore("Cluster does not support ANSI joins, test skipped.");
            }



            var query =
                from route in context.Query<Route>()
                join airport in context.Query<Airport>()
                        .UseHash(HashHintType.Build)
                        .UseIndex("def_faa")
                        .Where(p => p.Type == "airport")
                    on route.DestinationAirport equals airport.Faa
                where route.Type == "route"
                select new { airport.AirportName, route.Airline };

            var results = query.Take(1).ToList();
            Assert.AreEqual(1, results.Count);

            foreach (var b in results)
            {
                Console.WriteLine("Route for airline {0} goes to {1}", b.Airline, b.AirportName);
            }
        }

        [Test]
        public void Map2PocoTests_Simple_Projections_TypeFilterAttribute()
        {
            var context = new BucketContext(TestSetup.Bucket);


            var beers = (from b in context.Query<BeerFiltered>()
                select new {type = b.Type}).
                AsEnumerable();

            Assert.True(beers.All(p => p.type == "beer"));
        }

        [Test]
        public void Map2PocoTests_Simple_Projections_TypeFilterRuntime()
        {
            DocumentFilterManager.SetFilter(new BreweryFilter());

            var context = new BucketContext(TestSetup.Bucket);


            var breweries = (from b in context.Query<Brewery>()
                select new {type = b.Type})
                .AsEnumerable();

            Assert.True(breweries.All(p => p.type == "brewery"));
        }

        public void Map2PocoTests_Simple_Projections_MetaWhere()
        {
            var context = new BucketContext(TestSetup.Bucket);


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
            var context = new BucketContext(TestSetup.Bucket);


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
            var context = new BucketContext(TestSetup.Bucket);


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
            var context = new BucketContext(TestSetup.Bucket);


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
            var context = new BucketContext(TestSetup.Bucket);


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
            var context = new BucketContext(TestSetup.Bucket);


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
        public async Task JoinTests_InnerJoin_IndexJoin()
        {
            var context = new BucketContext(TestSetup.Bucket);

            await EnsureIndexExists(context.Bucket, "brewery_id", "brewery_id");

            var versionProvider = TestSetup.Cluster.ClusterServices.GetRequiredService<IClusterVersionProvider>();
            var clusterVersion = await versionProvider.GetVersionAsync() ?? FeatureVersions.DefaultVersion;
            if (clusterVersion < FeatureVersions.IndexJoin)
            {
                Assert.Ignore("Cluster does not support index joins, test skipped.");
            }

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
        public async Task JoinTests_InnerJoin_AnsiJoin()
        {
            var context = new BucketContext(_travelSample);

            var versionProvider = TestSetup.Cluster.ClusterServices.GetRequiredService<IClusterVersionProvider>();
            var clusterVersion = await versionProvider.GetVersionAsync() ?? FeatureVersions.DefaultVersion;
            if (clusterVersion < FeatureVersions.AnsiJoin)
            {
                Assert.Ignore("Cluster does not support ANSI joins, test skipped.");
            }

            var routes = from route in context.Query<Route>()
                join airport in context.Query<Airport>()
                    on route.DestinationAirport equals airport.Faa
                where (route.Type == "route") && (airport.Type == "airport")
                select new { airport.AirportName, route.Airline };

            var results = routes.Take(1).ToList();
            Assert.AreEqual(1, results.Count);

            foreach (var b in results)
            {
                Console.WriteLine("Route for airline {0} goes to {1}", b.Airline, b.AirportName);
            }
        }

        [Test]
        public async Task JoinTests_InnerJoin_AnsiJoinPrefiltered()
        {
            var context = new BucketContext(_travelSample);

            var versionProvider = TestSetup.Cluster.ClusterServices.GetRequiredService<IClusterVersionProvider>();
            var clusterVersion = await versionProvider.GetVersionAsync() ?? FeatureVersions.DefaultVersion;
            if (clusterVersion < FeatureVersions.AnsiJoin)
            {
                Assert.Ignore("Cluster does not support ANSI joins, test skipped.");
            }

            var routes = from route in context.Query<Route>().Where(p => p.Type == "route")
                join airport in context.Query<Airport>().Where(p => p.Type == "airport")
                    on route.DestinationAirport equals airport.Faa
                select new { airport.AirportName, route.Airline };

            var results = routes.Take(1).ToList();
            Assert.AreEqual(1, results.Count);

            foreach (var b in results)
            {
                Console.WriteLine("Route for airline {0} goes to {1}", b.Airline, b.AirportName);
            }
        }

        [Test]
        public void JoinTests_LeftJoin_Simple()
        {
            var context = new BucketContext(TestSetup.Bucket);

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
            var context = new BucketContext(TestSetup.Bucket);

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
            var context = new BucketContext(TestSetup.Bucket);

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
        public async Task JoinTests_LeftJoin_IndexJoin()
        {
            var context = new BucketContext(TestSetup.Bucket);

            await EnsureIndexExists(context.Bucket, "brewery_id", "brewery_id");

            var versionProvider = TestSetup.Cluster.ClusterServices.GetRequiredService<IClusterVersionProvider>();
            var clusterVersion = await versionProvider.GetVersionAsync() ?? FeatureVersions.DefaultVersion;
            if (clusterVersion < FeatureVersions.IndexJoin)
            {
                Assert.Ignore("Cluster does not support index joins, test skipped.");
            }



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
        public async Task JoinTests_LeftJoin_AnsiJoin()
        {
            var context = new BucketContext(_travelSample);

            var versionProvider = TestSetup.Cluster.ClusterServices.GetRequiredService<IClusterVersionProvider>();
            var clusterVersion = await versionProvider.GetVersionAsync() ?? FeatureVersions.DefaultVersion;
            if (clusterVersion < FeatureVersions.AnsiJoin)
            {
                Assert.Ignore("Cluster does not support ANSI joins, test skipped.");
            }

            var routes = from route in context.Query<Route>()
                join airport in context.Query<Airport>()
                    on route.DestinationAirport equals airport.Faa into ra
                from airport in ra.DefaultIfEmpty()
                where (route.Type == "route") && (airport.Type == "airport")
                select new { airport.AirportName, route.Airline };

            var results = routes.Take(1).ToList();
            Assert.AreEqual(1, results.Count);

            foreach (var b in results)
            {
                Console.WriteLine("Route for airline {0} goes to {1}", b.Airline, b.AirportName);
            }
        }

        [Test]
        public async Task JoinTests_LeftJoin_AnsiJoinPrefiltered()
        {
            var context = new BucketContext(_travelSample);

            var versionProvider = TestSetup.Cluster.ClusterServices.GetRequiredService<IClusterVersionProvider>();
            var clusterVersion = await versionProvider.GetVersionAsync() ?? FeatureVersions.DefaultVersion;
            if (clusterVersion < FeatureVersions.AnsiJoin)
            {
                Assert.Ignore("Cluster does not support ANSI joins, test skipped.");
            }

            var routes = from route in context.Query<Route>().Where(p => p.Type == "route")
                join airport in context.Query<Airport>().Where(p => p.Type == "airport")
                    on route.DestinationAirport equals airport.Faa into ra
                from airport in ra.DefaultIfEmpty()
                select new { airport.AirportName, route.Airline };

            var results = routes.Take(1).ToList();
            Assert.AreEqual(1, results.Count);

            foreach (var b in results)
            {
                Console.WriteLine("Route for airline {0} goes to {1}", b.Airline, b.AirportName);
            }
        }

        [Test]
        public void NestTests_Unnest_Simple()
        {
            var context = new BucketContext(TestSetup.Bucket);


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
            var context = new BucketContext(TestSetup.Bucket);


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
        public void NestTests_Unnest_Scalar()
        {
            var context = new BucketContext(TestSetup.Bucket);


            var breweries = from brewery in context.Query<Brewery>()
                            from address in brewery.Address
                            select address;

            var results = breweries.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var b in results)
            {
                Console.WriteLine(b);
            }
        }

        [Test]
        public async Task NestTests_Nest_IndexJoin()
        {
            var context = new BucketContext(TestSetup.Bucket);

            await EnsureIndexExists(context.Bucket, "brewery_id", "brewery_id");

            var versionProvider = TestSetup.Cluster.ClusterServices.GetRequiredService<IClusterVersionProvider>();
            var clusterVersion = await versionProvider.GetVersionAsync() ?? FeatureVersions.DefaultVersion;
            if (clusterVersion < FeatureVersions.IndexJoin)
            {
                Assert.Ignore("Cluster does not support index joins, test skipped.");
            }

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
        public async Task NestTests_Nest_IndexJoinPrefiltered()
        {
            var context = new BucketContext(TestSetup.Bucket);

            await EnsureIndexExists(context.Bucket, "brewery_id", "brewery_id");

            var versionProvider = TestSetup.Cluster.ClusterServices.GetRequiredService<IClusterVersionProvider>();
            var clusterVersion = await versionProvider.GetVersionAsync() ?? FeatureVersions.DefaultVersion;
            if (clusterVersion < FeatureVersions.IndexJoin)
            {
                Assert.Ignore("Cluster does not support index joins, test skipped.");
            }

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
        public async Task Test_AnsiNest_Prefiltered()
        {
            var context = new BucketContext(_travelSample);

            var versionProvider = TestSetup.Cluster.ClusterServices.GetRequiredService<IClusterVersionProvider>();
            var clusterVersion = await versionProvider.GetVersionAsync() ?? FeatureVersions.DefaultVersion;
            if (clusterVersion < FeatureVersions.AnsiJoin)
            {
                Assert.Ignore("Cluster does not support ANSI joins, test skipped.");
            }

            var query = from airline in context.Query<Airline>()
                join route in context.Query<Route>()
                        .Where(route => route.Type == "route" && route.SourceAirport == "SFO")
                    on airline.Iata equals route.Airline into routes
                where airline.Type == "airline" && airline.Country == "United States"
                select new { name = airline.Name, routes };

            var results = query.Take(1).ToList();
            Assert.AreEqual(1, results.Count());

            foreach (var airline in results)
            {
                Console.WriteLine("Airline {0} flies to these cities from SFO: {1}", airline.name,
                    string.Join(", ", airline.routes.Select(p => p.DestinationAirport)));
            }
        }

        [Test]
        public void SubqueryTests_ArraySubqueryWithFilter()
        {
            var context = new BucketContext(TestSetup.Bucket);

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
            var context = new BucketContext(TestSetup.Bucket);

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
            var context = new BucketContext(TestSetup.Bucket);

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
            var context = new BucketContext(TestSetup.Bucket);

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
            var context = new BucketContext(TestSetup.Bucket);

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
            var context = new BucketContext(TestSetup.Bucket);

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
            var context = new BucketContext(TestSetup.Bucket);

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
        public void AggregateTests_SimpleCount()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var count = context.Query<Beer>().Count(p => p.Type == "beer");
            Assert.Greater(count, 0);
            Console.WriteLine("Number of beers is {0}", count);
        }

        [Test]
        public void AggregateTests_GroupBy()
        {
            var context = new BucketContext(TestSetup.Bucket);

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
            var context = new BucketContext(TestSetup.Bucket);

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
            var context = new BucketContext(TestSetup.Bucket);

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
            var context = new BucketContext(TestSetup.Bucket);

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

        #region "Date/time functions"

        [Test]
        public void DateTime_DateAdd()
        {
            var context = new BucketContext(TestSetup.Bucket);

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
        public void DateTime_DateAdd_UnixMillis()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var beers = from beer in context.Query<Beer>()
                        where beer.Type == "beer" && N1QlFunctions.IsValued(beer.UpdatedUnixMillis)
                        select new { beer.Name, Updated = N1QlFunctions.DateAdd(beer.UpdatedUnixMillis.Value, -10, N1QlDatePart.Day) };

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
            var context = new BucketContext(TestSetup.Bucket);

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
        public void DateTime_DateDiff_UnixMillis()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var beers = from beer in context.Query<Beer>()
                        where beer.Type == "beer"
                        select new { beer.Name, DaysOld = N1QlFunctions.DateDiff(DateTime.Now, beer.UpdatedUnixMillis.Value, N1QlDatePart.Day) };

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
            var context = new BucketContext(TestSetup.Bucket);

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
        public void DateTime_DatePart_UnixMillis()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var beers = from beer in context.Query<Beer>()
                        where beer.Type == "beer"
                        select new { beer.Name, Year = N1QlFunctions.DatePart(beer.UpdatedUnixMillis.Value, N1QlDatePart.Year) };

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
            var context = new BucketContext(TestSetup.Bucket);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name, Updated = N1QlFunctions.DateTrunc(beer.Updated, N1QlDatePart.Month)};

            foreach (var b in beers.Take(1))
            {
                Console.WriteLine("Beer {0} is in {1:MMMM yyyy}", b.Name, b.Updated);
            }
        }

        [Test]
        public async Task DateTime_DateTrunc_UnixMillis()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var versionProvider = TestSetup.Cluster.ClusterServices.GetRequiredService<IClusterVersionProvider>();
            var clusterVersion = await versionProvider.GetVersionAsync() ?? FeatureVersions.DefaultVersion;
            if (clusterVersion.Version == new Version(5, 5, 0))
            {
                Assert.Ignore("Skipping temporarily due to bug in 5.5 Beta https://issues.couchbase.com/browse/MB-29357");
            }

            var beers = from beer in context.Query<Beer>()
                        where beer.Type == "beer" && N1QlFunctions.IsValued(beer.UpdatedUnixMillis)
                        select new { beer.Name, Updated = N1QlFunctions.DateTrunc(beer.UpdatedUnixMillis.Value, N1QlDatePart.Month) };

            foreach (var b in beers.Take(1))
            {
                Console.WriteLine("Beer {0} is in {1:MMMM yyyy}", b.Name, b.Updated);
            }
        }

        private async Task PrepareBeerDocuments()
        {
            var query = @"UPDATE `beer-sample` SET updatedUnixMillis = STR_TO_MILLIS(updated)
                  WHERE type = 'beer' AND updateUnixMillis IS MISSING";

            await TestSetup.Cluster.QueryAsync<dynamic>(query);
        }

        #endregion

        #region Dictionary

        [Test]
        public void DictionaryTests_Indexer()
        {
            var context = new BucketContext(TestSetup.Bucket);


            var breweries =
                from brewery in context.Query<Dictionary<string, object>>()
                where brewery["type"].ToString() == "brewery"
                select brewery;

            var results = breweries.Take(1).ToList();
            Assert.AreEqual(1, results.Count);
        }

        [Test]
        public void DictionaryTests_ContainsKey()
        {
            var context = new BucketContext(TestSetup.Bucket);


            var breweries =
                from brewery in context.Query<Dictionary<string, object>>()
                where brewery["type"].ToString() == "brewery" && brewery.ContainsKey("address")
                select brewery;

            var results = breweries.Take(1).ToList();
            Assert.AreEqual(1, results.Count);
        }

        [Test]
        public void DictionaryTests_Keys()
        {
            var context = new BucketContext(TestSetup.Bucket);


            var breweries =
                from brewery in context.Query<Dictionary<string, object>>()
                where brewery["type"].ToString() == "brewery"
                select brewery.Keys.ToList();

            var results = breweries.Take(1).ToList();
            Assert.AreEqual(1, results.Count);
            Assert.Greater(results[0].Count, 0);
        }

        [Test]
        public void DictionaryTests_Values()
        {
            var context = new BucketContext(TestSetup.Bucket);


            var breweries =
                from brewery in context.Query<Dictionary<string, object>>()
                where brewery["type"].ToString() == "brewery"
                select brewery.Values.ToList();

            var results = breweries.Take(1).ToList();
            Assert.AreEqual(1, results.Count);
            Assert.Greater(results[0].Count, 0);
        }

        #endregion
    }
}
