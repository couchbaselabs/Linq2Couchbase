using System;
using NUnit.Framework;

namespace Couchbase.Linq.IntegrationTests
{
    [SetUpFixture]
    public class TestSetup
    {
        [SetUp]
        public void SetUp()
        {
            ClusterHelper.Initialize(TestConfigurations.DefaultConfig());
        }
    }
}