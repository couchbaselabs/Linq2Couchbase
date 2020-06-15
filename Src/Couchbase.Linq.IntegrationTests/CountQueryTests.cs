using System;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.IntegrationTests.Documents;
using NUnit.Framework;

namespace Couchbase.Linq.IntegrationTests
{
    [TestFixture]
    public class CountQueryTests : N1QlTestBase
    {
        [Test]
        public void Count_HasResult()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name};

            Console.WriteLine(beers.Count());
        }

        [Test]
        public async Task CountAsync_HasResult()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name};

            Console.WriteLine(await beers.CountAsync());
        }

        [Test]
        public async Task CountAsync_WithPredicate_HasResult()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name};

            var result = await beers.CountAsync(p => p.Name == "21A IPA");

            Console.WriteLine(result);
        }

        [Test]
        public void LongCount_HasResult()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name};

            Console.WriteLine(beers.LongCount());
        }

        [Test]
        public async Task LongCountAsync_HasResult()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name};

            Console.WriteLine(await beers.LongCountAsync());
        }

        [Test]
        public async Task LongCountAsync_WithPredicate_HasResult()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name};

            var result = await beers.LongCountAsync(p => p.Name == "21A IPA");

            Console.WriteLine(result);
        }
    }
}
