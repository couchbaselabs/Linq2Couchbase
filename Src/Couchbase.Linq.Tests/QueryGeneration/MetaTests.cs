using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Metadata;
using Couchbase.Linq.Tests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.Tests.QueryGeneration
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
                .Select(p => N1Ql.Meta(p));

            const string expected = "SELECT META(p) FROM default as p";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Meta_Keyword_With_Projection()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Select(c=> new {c.Age, Meta = N1Ql.Meta(c)});


            const string expected = "SELECT c.age as Age, META(c) as Meta FROM default as c";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Meta_Fakeable()
        {
            // Confirms that the N1Ql.Meta operation can be faked for unit testing in a client application

            var metadata = new DocumentMetadata()
            {
                Id = "testid"
            };

            var mockObject = new Mock<Brewery>();

            var mockMetadataProvider = mockObject.As<IDocumentMetadataProvider>();
            mockMetadataProvider.Setup(p => p.GetMetadata()).Returns(metadata);

            var data = (new List<Brewery> {mockObject.Object}).AsQueryable();

            var query = from p in data select N1Ql.Meta(p).Id;

            Assert.AreEqual(metadata.Id, query.First());
        }
    }
}
