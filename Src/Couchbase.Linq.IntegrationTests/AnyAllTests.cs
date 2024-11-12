using System.Linq;
using System.Threading.Tasks;
using Couchbase.Core.Version;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.IntegrationTests.Documents;
using Couchbase.Linq.Utils;
using Couchbase.Linq.Versioning;
using NUnit.Framework;

namespace Couchbase.Linq.IntegrationTests
{
    [TestFixture]
    public class AnyAllTests : N1QlTestBase
    {
        [Test]
        public void AnyNestedArrayWithFilter()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var breweries = (from b in context.Query<Brewery>()
                where b.Type == "brewery" && b.Address.Any(p => p == "563 Second Street")
                select new {name = b.Name, address = b.Address}).
                Take(1).
                ToList();

            Assert.IsNotEmpty(breweries);
            Assert.True(breweries.All(p => p.address.Contains("563 Second Street")));
        }

        [Test]
        public async Task AnyNestedArrayWithFilter_UsesArrayIndex()
        {
            var context = new BucketContext(TestSetup.Bucket);

            // This test requires the following index:
            //   CREATE INDEX brewery_address ON `beer-sample`.`_default`.`_default` (DISTINCT ARRAY x FOR x IN address END) WHERE type = 'brewery'

            // It can't be automatically created currently because the bucket manager
            // doesn't support creating array or function-based indexes, only plain attribute indexes



            var explanation = await (from b in context.Query<Brewery>()
                               where b.Type == "brewery" && b.Address.Any(p => p == "563 Second Street")
                               select new { name = b.Name, address = b.Address }).
                ExplainAsync();

            Assert.AreEqual("DistinctScan", explanation.plan["~children"][0]["#operator"].ToString());
            Assert.True(explanation.plan["~children"][0].scan["#operator"].ToString().StartsWith("IndexScan"));
            Assert.AreEqual("brewery_address", explanation.plan["~children"][0].scan.index.ToString());
        }

        [Test]
        public void AnyOnMainDocument_ReturnsTrue()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var hasBreweries = (from b in context.Query<Brewery>()
                where b.Type == "brewery"
                select new {name = b.Name, address = b.Address}).Take(1).
                Any();

            Assert.True(hasBreweries);
        }

        [Test]
        public void AnyOnMainDocument_ReturnsFalse()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var hasFaketype = (from b in context.Query<Brewery>()
                where b.Type == "faketype"
                select new {name = b.Name, address = b.Address}).
                Take(1).
                Any();

            Assert.False(hasFaketype);
        }

        [Test]
        public async Task AnyAsyncOnMainDocument_ReturnsTrue()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var hasBreweries = await (from b in context.Query<Brewery>()
                    where b.Type == "brewery"
                    select new {name = b.Name, address = b.Address}).Take(1).
                AnyAsync();

            Assert.True(hasBreweries);
        }

        [Test]
        public async Task AnyAsyncOnMainDocument_ReturnsFalse()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var hasFaketype = await (from b in context.Query<Brewery>()
                    where b.Type == "faketype"
                    select new {name = b.Name, address = b.Address}).
                AnyAsync();

            Assert.False(hasFaketype);
        }

        [Test]
        public void AllNestedArray()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var breweries = (from b in context.Query<Brewery>()
                where b.Type == "brewery" && b.Address.All(p => p == "563 Second Street")
                select new {name = b.Name, address = b.Address}).
                Take(1).
                ToList();

            Assert.IsNotEmpty(breweries);
            Assert.True(breweries.SelectMany(p => p.address).All(p => p == "563 Second Street"));
        }

        [Test]
        public void AllNestedArrayPrefiltered()
        {
            var context = new BucketContext(TestSetup.Bucket);

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
        public void AllOnMainDocument_ReturnsFalse()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var isAllBreweries = context.Query<Brewery>().All(p => p.Type == "brewery");

            Assert.False(isAllBreweries);
        }

        [Test]
        public void AllOnMainDocument_ReturnsTrue()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var allBreweriesHaveAddress = (from b in context.Query<Brewery>()
                where b.Type == "brewery"
                select new {b.Name})
                .All(p => p.Name != "");

            Assert.True(allBreweriesHaveAddress);
        }

        [Test]
        public async Task AllAsyncOnMainDocument_ReturnsFalse()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var isAllBreweries = await context.Query<Brewery>().AllAsync(p => p.Type == "brewery");

            Assert.False(isAllBreweries);
        }

        [Test]
        public async Task AllAsyncOnMainDocument_ReturnsTrue()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var allBreweriesHaveAddress = await (from b in context.Query<Brewery>()
                    where b.Type == "brewery"
                    select new {b.Name})
                .AllAsync(p => p.Name != "");

            Assert.True(allBreweriesHaveAddress);
        }
    }
}
