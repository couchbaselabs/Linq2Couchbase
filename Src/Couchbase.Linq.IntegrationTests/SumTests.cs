using System;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.IntegrationTests.Documents;
using NUnit.Framework;

namespace Couchbase.Linq.IntegrationTests
{
    [TestFixture]
    public class SumTests : N1QlTestBase
    {
        [Test]
        public void Sum_NoSelector()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select beer.Ibu;

            Console.WriteLine(beers.Sum());
        }

        [Test]
        public async Task SumAsync_NoSelector()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select beer.Ibu;

            Console.WriteLine(await beers.SumAsync());
        }

        [Test]
        public async Task SumAsync_WithSelector()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select beer;

            Console.WriteLine(await beers.SumAsync(p => p.Ibu));
        }
    }
}
