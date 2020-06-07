using System;
using System.Threading.Tasks;
using Couchbase.KeyValue;
using NUnit.Framework;

namespace Couchbase.Linq.IntegrationTests
{
    [SetUpFixture]
    public class TestSetup : N1QlTestBase
    {
        public static ICluster Cluster { get; private set; }

        public static ICouchbaseCollection Collection { get; private set; }

        [OneTimeSetUp]
        public async Task SetUp()
        {
            Cluster = await Couchbase.Cluster.ConnectAsync(TestConfigurations.DefaultConfig());
            await Cluster.WaitUntilReadyAsync(TimeSpan.FromSeconds(10));

            var bucket = await Cluster.BucketAsync("beer-sample");
            await EnsurePrimaryIndexExists(bucket);

            Collection = bucket.DefaultCollection();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Cluster.Dispose();

            Cluster = null;
            Collection = null;
        }
    }
}