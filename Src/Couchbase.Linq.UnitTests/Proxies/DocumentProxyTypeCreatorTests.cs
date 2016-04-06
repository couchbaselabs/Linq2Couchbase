using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.Proxies;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.Proxies
{
    [TestFixture]
    public class DocumentProxyTypeCreatorTests
    {
        #region CanCreateObject

        [Test]
        public void CanCreateObject_POCO_ReturnsTrue()
        {
            // Arrange

            var creator = new DocumentProxyTypeCreator();

            // Act

            var result = creator.CanCreateObject(typeof (Poco));

            // Assert

            Assert.IsTrue(result);
        }

        [Test]
        public void CanCreateObject_SealedPOCO_ReturnsTrue()
        {
            // Arrange

            var creator = new DocumentProxyTypeCreator();

            // Act

            var result = creator.CanCreateObject(typeof(SealedPoco));

            // Assert

            Assert.IsFalse(result);
        }

        [Test]
        public void CanCreateObject_IList_ReturnsTrue()
        {
            // Arrange

            var creator = new DocumentProxyTypeCreator();

            // Act

            var result = creator.CanCreateObject(typeof(IList<Poco>));

            // Assert

            Assert.IsTrue(result);
        }

        [Test]
        public void CanCreateObject_ICollection_ReturnsTrue()
        {
            // Arrange

            var creator = new DocumentProxyTypeCreator();

            // Act

            var result = creator.CanCreateObject(typeof(ICollection<Poco>));

            // Assert

            Assert.IsTrue(result);
        }

        #endregion

        #region CreateObject

        [Test]
        public void CreateObject_POCO_ReturnsProxy()
        {
            // Arrange

            var creator = new DocumentProxyTypeCreator();

            // Act

            var result = creator.CreateObject(typeof(Poco));

            // Assert

            Assert.IsNotNull(result as ITrackedDocumentNode);
        }

        [Test]
        public void CreateObject_IList_ReturnsDocumentCollection()
        {
            // Arrange

            var creator = new DocumentProxyTypeCreator();

            // Act

            var result = creator.CreateObject(typeof(IList<Poco>));

            // Assert

            Assert.IsNotNull(result as DocumentCollection<Poco>);
        }

        [Test]
        public void CreateObject_ICollection_ReturnsDocumentCollection()
        {
            // Arrange

            var creator = new DocumentProxyTypeCreator();

            // Act

            var result = creator.CreateObject(typeof(ICollection<Poco>));

            // Assert

            Assert.IsNotNull(result as DocumentCollection<Poco>);
        }

        #endregion

        #region Helpers

        public class Poco
        {
        }

        public sealed class SealedPoco
        {
        }

        #endregion
    }
}
