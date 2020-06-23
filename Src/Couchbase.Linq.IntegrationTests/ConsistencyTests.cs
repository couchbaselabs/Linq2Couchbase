using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.IntegrationTests.Documents;
using Couchbase.Query;
using NUnit.Framework;

namespace Couchbase.Linq.IntegrationTests
{
    [TestFixture]
    public class ConsistencyTests : N1QlTestBase
    {
        [Test]
        public async Task ScanConsistency()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var beers = from b in context.Query<Beer>().ScanConsistency(QueryScanConsistency.RequestPlus)
                select b;

            var beer = await beers.FirstAsync();
            Console.WriteLine(beer.Name);
        }

        [Test]
        public async Task ConsistentWith()
        {
            var context = new BucketContext(TestSetup.Bucket);

            var upsertResult = await TestSetup.Bucket.DefaultCollection().UpsertAsync("test-mutation", new {a = "a"});
            try
            {
                var mutationState = MutationState.From(upsertResult);

                var beers = from b in context.Query<Beer>().ConsistentWith(mutationState)
                    select b;

                var beer = await beers.FirstAsync();
                Console.WriteLine(beer.Name);
            }
            finally
            {
                await TestSetup.Bucket.DefaultCollection().RemoveAsync("test-mutation");
            }
        }
    }
}
