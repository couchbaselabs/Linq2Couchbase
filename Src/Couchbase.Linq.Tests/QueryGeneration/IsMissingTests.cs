using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Tests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.Tests.QueryGeneration
{
    [TestFixture]
    public class IsMissingTests : N1QLTestBase
    {
        [Test]
        public void Test_IsMissing_ByProperty()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Where(p => N1Ql.IsMissing(p.Age));

            const string expected = "SELECT p.* FROM default as p WHERE p.age IS MISSING";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IsMissing_ByName()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Where(p => N1Ql.IsMissing(p, "test"));

            const string expected = "SELECT p.* FROM default as p WHERE p.test IS MISSING";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IsNotMissing_ByProperty()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Where(p => N1Ql.IsNotMissing(p.Age));

            const string expected = "SELECT p.* FROM default as p WHERE p.age IS NOT MISSING";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IsNotMissing_ByName()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Where(p => N1Ql.IsNotMissing(p, "test"));

            const string expected = "SELECT p.* FROM default as p WHERE p.test IS NOT MISSING";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IsMissing_SubstiteWithDefault()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Select(p => new {age = N1Ql.IsMissing(p.Age) ? 10 : p.Age});

            const string expected = "SELECT CASE WHEN p.age IS MISSING THEN 10 ELSE p.age END as age FROM default as p";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IsValued_ByProperty()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Where(p => N1Ql.IsValued(p.Age));

            const string expected = "SELECT p.* FROM default as p WHERE p.age IS VALUED";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IsValued_ByName()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Where(p => N1Ql.IsValued(p, "test"));

            const string expected = "SELECT p.* FROM default as p WHERE p.test IS VALUED";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IsNotValued_ByProperty()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Where(p => N1Ql.IsNotValued(p.Age));

            const string expected = "SELECT p.* FROM default as p WHERE p.age IS NOT VALUED";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IsNotValued_ByName()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Where(p => N1Ql.IsNotValued(p, "test"));

            const string expected = "SELECT p.* FROM default as p WHERE p.test IS NOT VALUED";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IsNotValued_SubstiteWithDefault()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Select(p => new { age = N1Ql.IsNotValued(p.Age) ? 10 : p.Age });

            const string expected = "SELECT CASE WHEN p.age IS NOT VALUED THEN 10 ELSE p.age END as age FROM default as p";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }
    }
}
