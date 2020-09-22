using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration
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
                .Where(p => N1QlFunctions.IsMissing(p.Age));

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE `Extent1`.`age` IS MISSING";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IsMissing_ByName()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Where(p => N1QlFunctions.IsMissing(p, "test"));

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE `Extent1`.`test` IS MISSING";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IsNotMissing_ByProperty()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Where(p => N1QlFunctions.IsNotMissing(p.Age));

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE `Extent1`.`age` IS NOT MISSING";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IsNotMissing_ByName()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Where(p => N1QlFunctions.IsNotMissing(p, "test"));

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE `Extent1`.`test` IS NOT MISSING";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IsMissing_SubstiteWithDefault()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Select(p => new {age = N1QlFunctions.IsMissing(p.Age) ? 10 : p.Age});

            const string expected = "SELECT CASE WHEN `Extent1`.`age` IS MISSING THEN 10 ELSE `Extent1`.`age` END as `age` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IsValued_ByProperty()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Where(p => N1QlFunctions.IsValued(p.Age));

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE `Extent1`.`age` IS VALUED";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IsValued_ByName()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Where(p => N1QlFunctions.IsValued(p, "test"));

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE `Extent1`.`test` IS VALUED";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IsNotValued_ByProperty()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Where(p => N1QlFunctions.IsNotValued(p.Age));

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE `Extent1`.`age` IS NOT VALUED";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IsNotValued_ByName()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Where(p => N1QlFunctions.IsNotValued(p, "test"));

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE `Extent1`.`test` IS NOT VALUED";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IsNotValued_SubstiteWithDefault()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Select(p => new { age = N1QlFunctions.IsNotValued(p.Age) ? 10 : p.Age });

            const string expected = "SELECT CASE WHEN `Extent1`.`age` IS NOT VALUED THEN 10 ELSE `Extent1`.`age` END as `age` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }
    }
}
