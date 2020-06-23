using System;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.IntegrationTests.Documents;
using NUnit.Framework;

namespace Couchbase.Linq.IntegrationTests
{
    [TestFixture]
    public class AverageTests : N1QlTestBase
    {
        [Test]
        public void Average_WithSelector()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var avg = context.Query<Beer>()
                .Where(p => p.Type == "beer" && N1QlFunctions.IsValued(p.Abv))
                .Average(p => p.Abv);

            Assert.Greater(avg, 0);
            Console.WriteLine("Average ABV of all beers is {0}", avg);
        }

        [Test]
        public async Task AverageAsync_NoSelector()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var avg = await context.Query<Beer>()
                .Where(p => p.Type == "beer" && N1QlFunctions.IsValued(p.Abv))
                .Select(p => p.Abv)
                .AverageAsync();

            Assert.Greater(avg, 0);
            Console.WriteLine("Average ABV of all beers is {0}", avg);
        }

        [Test]
        public async Task AverageAsync_WithSelector()
        {
            var context = new CollectionContext(TestSetup.Collection);

            var avg = await context.Query<Beer>()
                .Where(p => p.Type == "beer" && N1QlFunctions.IsValued(p.Abv))
                .AverageAsync(p => p.Abv);

            Assert.Greater(avg, 0);
            Console.WriteLine("Average ABV of all beers is {0}", avg);
        }
    }
}
