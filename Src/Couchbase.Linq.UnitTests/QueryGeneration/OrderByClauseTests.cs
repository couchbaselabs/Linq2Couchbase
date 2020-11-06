using System.Linq;
using Couchbase.Core;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    internal class OrderByClauseTests : N1QLTestBase
    {
        [Test]
        public void Test_Where_With_OrderBy()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Age > 10 && e.FirstName == "Sam")
                    .OrderBy(e => e.Age)
                    .Select(e => new {age = e.Age, name = e.FirstName});


            const string expected =
                "SELECT `Extent1`.`age` as `age`, `Extent1`.`fname` as `name` FROM `default` as `Extent1` WHERE ((`Extent1`.`age` > 10) AND (`Extent1`.`fname` = 'Sam')) ORDER BY `Extent1`.`age` ASC";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Where_With_Multiple_OrderBy()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .OrderBy(e => e.Age)
                    .ThenByDescending(e => e.Email)
                    .Select(e => new {age = e.Age});


            const string expected = "SELECT `Extent1`.`age` as `age` FROM `default` as `Extent1` ORDER BY `Extent1`.`age` ASC, `Extent1`.`email` DESC";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Where_With_Descending_Then_Ascending()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .OrderByDescending(e => e.Age)
                    .ThenBy(e => e.Email)
                    .Select(e => new {age = e.Age});


            const string expected = "SELECT `Extent1`.`age` as `age` FROM `default` as `Extent1` ORDER BY `Extent1`.`age` DESC, `Extent1`.`email` ASC";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #region date/time

        [Test]
        public void Test_OrderBy_DateTime_Ascending()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .OrderBy(e => e.Updated);

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` ORDER BY STR_TO_MILLIS(`Extent1`.`updated`) ASC";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_OrderBy_DateTime_Descending()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .OrderByDescending(e => e.Updated);

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` ORDER BY STR_TO_MILLIS(`Extent1`.`updated`) DESC";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_ThenBy_DateTime_Ascending()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .OrderBy(e => e.Name)
                    .ThenBy(e => e.Updated);

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` ORDER BY `Extent1`.`name` ASC, STR_TO_MILLIS(`Extent1`.`updated`) ASC";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_ThenBy_DateTime_Descending()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .OrderBy(e => e.Name)
                    .ThenByDescending(e => e.Updated);

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` ORDER BY `Extent1`.`name` ASC, STR_TO_MILLIS(`Extent1`.`updated`) DESC";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_OrderBy_DateTimeUnixMillis_Ascending()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .OrderBy(e => e.UpdatedUnixMillis);

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` ORDER BY `Extent1`.`updatedUnixMillis` ASC";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_OrderBy_DateTimeUnixMillis_Descending()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .OrderByDescending(e => e.UpdatedUnixMillis);

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` ORDER BY `Extent1`.`updatedUnixMillis` DESC";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_ThenBy_DateTimeUnixMillis_Ascending()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .OrderBy(e => e.Name)
                    .ThenBy(e => e.UpdatedUnixMillis);

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` ORDER BY `Extent1`.`name` ASC, `Extent1`.`updatedUnixMillis` ASC";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_ThenBy_DateTimeUnixMillis_Descending()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .OrderBy(e => e.Name)
                    .ThenByDescending(e => e.UpdatedUnixMillis);

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` ORDER BY `Extent1`.`name` ASC, `Extent1`.`updatedUnixMillis` DESC";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion
    }
}