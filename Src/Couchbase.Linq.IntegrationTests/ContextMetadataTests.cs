using Couchbase.Linq.IntegrationTests.Documents;
using Couchbase.Linq.Metadata;
using NUnit.Framework;

namespace Couchbase.Linq.IntegrationTests
{
    [TestFixture]
    public class ContextMetadataTests : N1QlTestBase
    {
        #region ctor

        [Test]
        public void ctor_GetsAllPublicDocumentSets()
        {
            // Act

            var result = new ContextMetadata(typeof(TestContext));

            // Assert

            Assert.AreEqual(3, result.Properties.Length);
        }

        [Test]
        public void ctor_ValidInitializer()
        {
            // Arrange

            var context = new TestContext(TestSetup.Bucket);
            var metadata = new ContextMetadata(typeof(TestContext));

            // Act

            metadata.Fill(context);

            // Assert

            Assert.NotNull(context.Beers);
            Assert.NotNull(context.Routes);
            Assert.NotNull(context.RoutesOverridden);
        }

        #endregion

        #region Helpers

        private class TestContext : BucketContext
        {
            public TestContext (IBucket bucket) : base(bucket) { }

            public IDocumentSet<Beer> Beers { get; set; }

            public IDocumentSet<RouteInCollection> Routes { get; set; }

            [CouchbaseCollection("my", "override")]
            public IDocumentSet<RouteInCollection> RoutesOverridden { get; set; }

            private IDocumentSet<Beer> PrivateBeers { get; set; }

            private IDocumentSet<Beer> NoSetterBeers => null;

            private int OtherType { get; set; }
        }

        #endregion
    }
}
