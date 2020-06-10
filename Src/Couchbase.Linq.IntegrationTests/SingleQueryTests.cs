using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.IntegrationTests.Documents;
using NUnit.Framework;

namespace Couchbase.Linq.IntegrationTests
{
    [TestFixture]
    public class SingleQueryTests : N1QlTestBase
    {
        [Test]
        public void Single_Empty()
        {
            var context = new CollectionContext(TestSetup.Collection);

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
        public void SingleAsync_Empty()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "abcdefg"
                select new {beer.Name};

            Assert.ThrowsAsync<InvalidOperationException>(beers.SingleAsync);
        }

        [Test]
        public void Single_HasResult()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Name == "21A IPA"
                select new {beer.Name};

            Console.WriteLine(beers.Single().Name);
        }

        [Test]
        public async Task SingleAsync_HasResult()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Name == "21A IPA"
                select new {beer.Name};

            Console.WriteLine((await beers.SingleAsync()).Name);
        }

        [Test]
        public async Task SingleAsync_WithPredicate_HasResult()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                select new {beer.Name};

            var result = await beers.SingleAsync(p => p.Name == "21A IPA");

            Console.WriteLine(result.Name);
        }

        [Test]
        public void Single_HasManyResults()
        {
            var context = new CollectionContext(TestSetup.Collection);

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
        public void SingleAsync_HasManyResults()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name};

            Assert.ThrowsAsync<InvalidOperationException>(beers.SingleAsync);
        }

        [Test]
        public void SingleOrDefault_Empty()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "abcdefg"
                select new {beer.Name};

            var aBeer = beers.SingleOrDefault();
            Assert.IsNull(aBeer);
        }

        [Test]
        public async Task SingleOrDefaultAsync_Empty()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "abcdefg"
                select new {beer.Name};

            var aBeer = await beers.SingleOrDefaultAsync();
            Assert.IsNull(aBeer);
        }

        [Test]
        public void SingleOrDefault_HasResult()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Name == "21A IPA"
                select new {beer.Name};

            var aBeer = beers.SingleOrDefault();
            Assert.IsNotNull(aBeer);
            Console.WriteLine(aBeer.Name);
        }

        [Test]
        public async Task SingleOrDefaultAsync_HasResult()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Name == "21A IPA"
                select new {beer.Name};

            var aBeer = await beers.SingleOrDefaultAsync();
            Assert.IsNotNull(aBeer);
            Console.WriteLine(aBeer.Name);
        }

        [Test]
        public async Task SingleOrDefaultAsync_WithPredicate_HasResult()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                select new {beer.Name};

            var aBeer = await beers.SingleOrDefaultAsync(p => p.Name == "21A IPA");
            Assert.IsNotNull(aBeer);
            Console.WriteLine(aBeer.Name);
        }

        [Test]
        public void SingleOrDefault_HasManyResults()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name};

            Assert.Throws<InvalidOperationException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var temp = beers.SingleOrDefault();
            });
        }

        [Test]
        public void SingleOrDefaultAsync_HasManyResults()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var beers = from beer in context.Query<Beer>()
                where beer.Type == "beer"
                select new {beer.Name};

            Assert.ThrowsAsync<InvalidOperationException>(beers.SingleOrDefaultAsync);
        }
    }
}
