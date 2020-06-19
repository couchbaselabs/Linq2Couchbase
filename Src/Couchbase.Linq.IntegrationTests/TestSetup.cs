using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Couchbase.Linq.IntegrationTests
{
    [SetUpFixture]
    public class TestSetup : N1QlTestBase
    {
        public static ICluster Cluster { get; private set; }

        public static IBucket Bucket { get; private set; }

        [OneTimeSetUp]
        public async Task SetUp()
        {
            Cluster = await Couchbase.Cluster.ConnectAsync(TestConfigurations.DefaultConfig());
            await Cluster.WaitUntilReadyAsync(TimeSpan.FromSeconds(10));

            var bucket = await Cluster.BucketAsync("beer-sample");
            await EnsurePrimaryIndexExists(bucket);

            Bucket = bucket;
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Cluster.Dispose();

            Cluster = null;
            Bucket = null;
        }
    }
}