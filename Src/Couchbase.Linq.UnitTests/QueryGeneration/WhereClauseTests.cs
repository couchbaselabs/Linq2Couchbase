using System;
using System.Linq;
using Couchbase.Core;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    public class WhereClauseTests : N1QLTestBase
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
        public void Test_Where_With_Missing()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Email == "something@gmail.com")
                    .Where(g => N1QlFunctions.IsMissing(g.Age))
                    .OrderBy(e => e.Age)
                    .Select(e => new {age = e.Age, name = e.FirstName});

            const string expected =
                "SELECT `Extent1`.`age` as `age`, `Extent1`.`fname` as `name` FROM `default` as `Extent1` WHERE (`Extent1`.`email` = 'something@gmail.com') AND `Extent1`.`age` IS MISSING ORDER BY `Extent1`.`age` ASC";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Where_With_Like()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Age > 10 && e.FirstName == "Sam" && e.LastName.Contains("a"))
                    .Select(e => new {age = e.Age, name = e.FirstName});

            const string expected =
                "SELECT `Extent1`.`age` as `age`, `Extent1`.`fname` as `name` FROM `default` as `Extent1` WHERE (((`Extent1`.`age` > 10) AND (`Extent1`.`fname` = 'Sam')) AND (`Extent1`.`lname` LIKE '%a%'))";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Where()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Age > 10 && e.FirstName == "Sam")
                    .Select(e => new {age = e.Age, name = e.FirstName});


            const string expected =
                "SELECT `Extent1`.`age` as `age`, `Extent1`.`fname` as `name` FROM `default` as `Extent1` WHERE ((`Extent1`.`age` > 10) AND (`Extent1`.`fname` = 'Sam'))";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Where_With_Parameters()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            const int age = 10;
            const string firstName = "Sam";

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Age > age && e.FirstName == firstName)
                    .Select(e => new {age = e.Age, name = e.FirstName});


            const string expected =
                "SELECT `Extent1`.`age` as `age`, `Extent1`.`fname` as `name` FROM `default` as `Extent1` WHERE ((`Extent1`.`age` > 10) AND (`Extent1`.`fname` = 'Sam'))";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Multiple_Where()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Age > 10 && e.FirstName == "Sam")
                    .Where(e => e.Email == "myemail@test.com")
                    .Select(e => new {age = e.Age, name = e.FirstName});

            const string expected =
                "SELECT `Extent1`.`age` as `age`, `Extent1`.`fname` as `name` FROM `default` as `Extent1` WHERE ((`Extent1`.`age` > 10) AND (`Extent1`.`fname` = 'Sam')) AND (`Extent1`.`email` = 'myemail@test.com')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Where_With_Or()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Age > 10 || e.FirstName == "Sam")
                    .Select(e => new {age = e.Age, name = e.FirstName});

            const string expected =
                "SELECT `Extent1`.`age` as `age`, `Extent1`.`fname` as `name` FROM `default` as `Extent1` WHERE ((`Extent1`.`age` > 10) OR (`Extent1`.`fname` = 'Sam'))";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Where_With_Math()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Age < 10 + 30)
                    .Select(e => new {age = e.Age, name = e.FirstName});

            const string expected = "SELECT `Extent1`.`age` as `age`, `Extent1`.`fname` as `name` FROM `default` as `Extent1` WHERE (`Extent1`.`age` < 40)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Where_With_IsNull()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.FirstName == null)
                    .Select(e => new { age = e.Age, name = e.FirstName });


            const string expected =
                "SELECT `Extent1`.`age` as `age`, `Extent1`.`fname` as `name` FROM `default` as `Extent1` WHERE (`Extent1`.`fname` IS NULL)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Where_With_IsNotNull()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.FirstName != null)
                    .Select(e => new { age = e.Age, name = e.FirstName });


            const string expected =
                "SELECT `Extent1`.`age` as `age`, `Extent1`.`fname` as `name` FROM `default` as `Extent1` WHERE (`Extent1`.`fname` IS NOT NULL)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Where_With_DateComparison()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .Where(e => e.Updated >= new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                    .Select(e => new { e.Name });


            const string expected =
                "SELECT `Extent1`.`name` as `Name` FROM `default` as `Extent1` " +
                "WHERE (STR_TO_MILLIS(`Extent1`.`updated`) >= STR_TO_MILLIS(\"2010-01-01T00:00:00Z\"))";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }
    }
}