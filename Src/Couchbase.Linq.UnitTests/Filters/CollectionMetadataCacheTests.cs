using System;
using Couchbase.Linq.Filters;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.Filters
{
    public class CollectionMetadataCacheTests
    {
        [Test]
        public void GetCollection_Annotated_AnnotationValues()
        {
            // Arrange

            var cache = new CollectionMetadataCache();

            // Act

            var (scope, collection) = cache.GetCollection<Annotated>();

            // Assert

            Assert.AreEqual("my_scope", scope);
            Assert.AreEqual("my_collection", collection);
        }

        [Test]
        public void GetCollection_Unannotated_DefaultValues()
        {
            // Arrange

            var cache = new CollectionMetadataCache();

            // Act

            var (scope, collection) = cache.GetCollection<Unannotated>();

            // Assert

            Assert.AreEqual("_default", scope);
            Assert.AreEqual("_default", collection);
        }

        #region Helpers

        [CouchbaseCollection("my_scope", "my_collection")]
        public class Annotated
        {
        }

        public class Unannotated
        {
        }

        #endregion
    }
}
