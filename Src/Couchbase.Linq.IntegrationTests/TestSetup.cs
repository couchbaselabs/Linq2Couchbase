using System;
using NUnit.Framework;

namespace Couchbase.Linq.IntegrationTests
{
    [SetUpFixture]
    public class TestSetup : N1QlTestBase
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            ClusterHelper.Initialize(TestConfigurations.DefaultConfig());

            EnsurePrimaryIndexExists(ClusterHelper.GetBucket("beer-sample"));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            ClusterHelper.Close();
        }
    }
}