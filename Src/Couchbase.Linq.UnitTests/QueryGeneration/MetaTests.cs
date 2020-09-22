using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Metadata;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    public class MetaTests : N1QLTestBase
    {
        [Test]
        public void Test_Meta_Keyword()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Select(p => N1QlFunctions.Meta(p));

            const string expected = "SELECT RAW META(`Extent1`) FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Meta_Keyword_With_Projection()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Select(c=> new {c.Age, Meta = N1QlFunctions.Meta(c)});


            const string expected = "SELECT `Extent1`.`age` as `Age`, META(`Extent1`) as `Meta` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Meta_Fakeable()
        {
            // Confirms that the N1QlFunctions.Meta operation can be faked for unit testing in a client application

            var metadata = new DocumentMetadata()
            {
                Id = "testid"
            };

            var mockObject = new Mock<Brewery>();

            var mockMetadataProvider = mockObject.As<IDocumentMetadataProvider>();
            mockMetadataProvider.Setup(p => p.GetMetadata()).Returns(metadata);

            var data = (new List<Brewery> {mockObject.Object}).AsQueryable();

            var query = from p in data select N1QlFunctions.Meta(p).Id;

            Assert.AreEqual(metadata.Id, query.First());
        }
    }
}
