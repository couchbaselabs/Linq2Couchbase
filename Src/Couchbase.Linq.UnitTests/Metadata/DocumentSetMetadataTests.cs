using System;
using Couchbase.Linq.Metadata;
using Couchbase.Linq.UnitTests.Documents;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.Metadata
{
    public class DocumentSetMetadataTests
    {
        #region ctor

        [Test]
        public void ctor_GetsCorrectDocumentType()
        {
            // Arrange

            var property = typeof(TestDoc).GetProperty(nameof(TestDoc.Beers));

            // Act

            var result = new DocumentSetMetadata(property!);

            // Assert

            Assert.AreEqual(typeof(Beer), result.DocumentType);
        }

        [Test]
        public void ctor_GetsDefaultCollection()
        {
            // Arrange

            var property = typeof(TestDoc).GetProperty(nameof(TestDoc.Beers));

            // Act

            var result = new DocumentSetMetadata(property!);

            // Assert

            Assert.AreEqual(CouchbaseCollectionAttribute.Default, result.CollectionInfo);
        }

        [Test]
        public void ctor_GetsDocumentCollectionAttribute()
        {
            // Arrange

            var property = typeof(TestDoc).GetProperty(nameof(TestDoc.Routes));

            // Act

            var result = new DocumentSetMetadata(property!);

            // Assert

            Assert.AreEqual("inventory", result.CollectionInfo.Scope);
            Assert.AreEqual("route", result.CollectionInfo.Collection);
        }

        [Test]
        public void ctor_PrefersPropertyCollectionAttribute()
        {
            // Arrange

            var property = typeof(TestDoc).GetProperty(nameof(TestDoc.RoutesOverridden));

            // Act

            var result = new DocumentSetMetadata(property!);

            // Assert

            Assert.AreEqual("my", result.CollectionInfo.Scope);
            Assert.AreEqual("override", result.CollectionInfo.Collection);
        }

        #endregion

        #region Helpers

        private class TestDoc
        {
            public IDocumentSet<Beer> Beers { get; set; }

            public IDocumentSet<RouteInCollection> Routes { get; set; }

            [CouchbaseCollection("my", "override")]
            public IDocumentSet<RouteInCollection> RoutesOverridden { get; set; }
        }

        #endregion
    }
}
