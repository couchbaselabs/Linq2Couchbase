using System.Linq;
using Couchbase.Core;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    public class TakeAndSkipTests : N1QLTestBase
    {
        [Test]
        public void Test_Take()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => new {age = e.Age, name = e.FirstName})
                    .Take(30);

            const string expected = "SELECT `Extent1`.`age` as `age`, `Extent1`.`fname` as `name` FROM `default` as `Extent1` LIMIT 30";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Skip_Without_Take()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => e)
                    .Skip(10);

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Skip_With_Take()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => e)
                    .Skip(10)
                    .Take(10);

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` LIMIT 10 OFFSET 10";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_First()
        {
            var temp = CreateQueryable<Contact>("default").First();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` LIMIT 1";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_FirstOrDefault()
        {
            var temp = CreateQueryable<Contact>("default").FirstOrDefault();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` LIMIT 1";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_FirstWithSkip()
        {
            var temp = CreateQueryable<Contact>("default").Skip(10).First();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` LIMIT 1 OFFSET 10";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Single()
        {
            var temp = CreateQueryable<Contact>("default").Single();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` LIMIT 2";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_SingleOrDefault()
        {
            var temp = CreateQueryable<Contact>("default").SingleOrDefault();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` LIMIT 2";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_SingleWithSkip()
        {
            var temp = CreateQueryable<Contact>("default").Skip(10).Single();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` LIMIT 2 OFFSET 10";

            Assert.AreEqual(expected, n1QlQuery);
        }
    }
}