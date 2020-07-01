using System;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.IntegrationTests.Documents;
using NUnit.Framework;

namespace Couchbase.Linq.IntegrationTests
{
    [TestFixture]
    public class MinMaxTests : N1QlTestBase
    {
        [Test]
        public void Min_WithSelector()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var min = context.Query<Beer>()
                .Where(p => p.Type == "beer" && N1QlFunctions.IsValued(p.Abv))
                .Min(p => p.Abv);

            Assert.AreEqual(min, 0);
            Console.WriteLine("Min ABV of all beers is {0}", min);
        }

        [Test]
        public async Task MinAsync_NoSelector()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var min = await context.Query<Beer>()
                .Where(p => p.Type == "beer" && N1QlFunctions.IsValued(p.Abv))
                .Select(p => p.Abv)
                .MinAsync();

            Assert.AreEqual(min, 0);
            Console.WriteLine("Min ABV of all beers is {0}", min);
        }

        [Test]
        public async Task MinAsync_WithSelector()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var min = await context.Query<Beer>()
                .Where(p => p.Type == "beer" && N1QlFunctions.IsValued(p.Abv))
                .MinAsync(p => p.Abv);

            Assert.AreEqual(min, 0);
            Console.WriteLine("Min ABV of all beers is {0}", min);
        }

        [Test]
        public void Max_WithSelector()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var max = context.Query<Beer>()
                .Where(p => p.Type == "beer" && N1QlFunctions.IsValued(p.Abv))
                .Max(p => p.Abv);

            Assert.Greater(max, 0);
            Console.WriteLine("Max ABV of all beers is {0}", max);
        }

        [Test]
        public async Task MaxAsync_NoSelector()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var max = await context.Query<Beer>()
                .Where(p => p.Type == "beer" && N1QlFunctions.IsValued(p.Abv))
                .Select(p => p.Abv)
                .MaxAsync();

            Assert.Greater(max, 0);
            Console.WriteLine("Max ABV of all beers is {0}", max);
        }

        [Test]
        public async Task MaxAsync_WithSelector()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var max = await context.Query<Beer>()
                .Where(p => p.Type == "beer" && N1QlFunctions.IsValued(p.Abv))
                .MaxAsync(p => p.Abv);

            Assert.Greater(max, 0);
            Console.WriteLine("Max ABV of all beers is {0}", max);
        }
    }
}
