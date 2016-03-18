using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.IntegrationTests.Documents;
using NUnit.Framework;

namespace Couchbase.Linq.IntegrationTests.Extensions
{
    [TestFixture]
    public class QueryExtensionTests
    {
        [Test]
        public async Task ExecuteAsync_NoParameters_ReturnsList()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from b in context.Query<Beer>()
                select b;

            var results = (await beers.Take(1).ExecuteAsync()).ToList();
            Assert.AreEqual(1, results.Count);

            foreach (var beer in results)
            {
                Console.WriteLine(beer.Name);
            }
        }

        [Test]
        public async Task ExecuteAsync_WithAvg_ReturnsAvg()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from b in context.Query<Beer>()
                        select b;

            var result = await beers.ExecuteAsync(p => p.Average(q => q.Abv));
            Console.WriteLine(result);
        }

        [Test]
        public async Task ExecuteAsync_First_ReturnsFirst()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from b in context.Query<Beer>()
                        select b;

            var result = await beers.ExecuteAsync(p => p.First());

            Assert.NotNull(result);
            Console.WriteLine(result.Name);
        }

        [Test]
        public async Task ExecuteAsync_FirstOrDefaultNoValues_ReturnsDefault()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from b in context.Query<Beer>()
                        where b.Name == "abcdefg"
                        select b.Abv;

            var result = await beers.ExecuteAsync(p => p.FirstOrDefault());

            Assert.AreEqual(0M, result);
        }

        [Test]
        public async Task ExecuteAsync_Any_ReturnsTrue()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from b in context.Query<Beer>()
                        where b.Type == "beer" && b.Name == "21A IPA"
                        select b;

            var result = await beers.ExecuteAsync(p => p.Any());

            Assert.True(result);
        }

        [Test]
        public async Task ExecuteAsync_All_ReturnsFalse()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");
            var context = new BucketContext(bucket);

            var beers = from b in context.Query<Beer>()
                        where b.Type == "beer"
                        select b;

            var result = await beers.ExecuteAsync(p => p.All(q => q.Name == "21A IPA"));

            Assert.False(result);
        }
    }
}