using System;
using System.Collections.Generic;
using System.Linq;
using Couchbase.Core;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    public class SelectDocumentMetadataTests : N1QLTestBase
    {
        [Test]
        public void Test_SelectDocumentMetadata_Basic()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object);

            const string expected = "SELECT `Extent1`.*, META(`Extent1`) as `__metadata` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression, true);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_SelectDocumentMetadata_WithPlainSelectProjection()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .Select(p => p);

            const string expected = "SELECT `Extent1`.*, META(`Extent1`) as `__metadata` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression, true);

            Assert.AreEqual(expected, n1QlQuery);
        }
    }
}