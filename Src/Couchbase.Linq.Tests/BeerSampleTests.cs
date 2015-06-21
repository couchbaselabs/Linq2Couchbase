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
        public void Map2PocoTests_Simple_Projections_WhereNot()
        {
            using (var cluster = new Cluster())
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var beers = from b in bucket.Queryable<Beer>()
                                where b.Type == "beer" && !(b.Abv < 4)
                                select new { name = b.Name, abv = b.Abv };

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
    }
}