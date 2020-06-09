using System;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.IntegrationTests.Documents;
using NUnit.Framework;

namespace Couchbase.Linq.IntegrationTests
{
    [TestFixture]
    public class FirstQueryTests : N1QlTestBase
    {
        [Test]
        public void First_Empty()
        {
            var context = new CollectionContext(TestSetup.Collection);

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
        public void FirstAsync_Empty()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "abcdefg"
                select new {beer.Name};

            Assert.ThrowsAsync<InvalidOperationException>(beers.FirstAsync);
        }

        [Test]
        public void First_HasResult()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name};

            Console.WriteLine(beers.First().Name);
        }

        [Test]
        public async Task FirstAsync_HasResult()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name};

            Console.WriteLine((await beers.FirstAsync()).Name);
        }

        [Test]
        public async Task FirstAsync_WithPredicate_HasResult()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name};

            var result = await beers.FirstAsync(p => p.Name != "21A IPA");

            Console.WriteLine(result.Name);
        }

        [Test]
        public void FirstOrDefault_Empty()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "abcdefg"
                select new {beer.Name};

            var aBeer = beers.FirstOrDefault();
            Assert.IsNull(aBeer);
        }

        [Test]
        public async Task FirstOrDefaultAsync_Empty()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "abcdefg"
                select new {beer.Name};

            var aBeer = await beers.FirstOrDefaultAsync();
            Assert.IsNull(aBeer);
        }

        [Test]
        public void FirstOrDefault_HasResult()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name};

            var aBeer = beers.FirstOrDefault();
            Assert.IsNotNull(aBeer);
            Console.WriteLine(aBeer.Name);
        }

        [Test]
        public async Task FirstOrDefaultAsync_HasResult()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name};

            var aBeer = await beers.FirstOrDefaultAsync();
            Assert.IsNotNull(aBeer);
            Console.WriteLine(aBeer.Name);
        }

        [Test]
        public async Task FirstOrDefaultAsync_WithPredicate_HasResult()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name};

            var aBeer = await beers.FirstOrDefaultAsync(p => p.Name != "21A IPA");
            Assert.IsNotNull(aBeer);
            Console.WriteLine(aBeer.Name);
        }
    }
}
