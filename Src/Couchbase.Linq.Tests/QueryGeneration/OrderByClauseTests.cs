using System.Linq;
using Couchbase.Core;
using Couchbase.Linq.Tests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.Tests.QueryGeneration
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
                "SELECT `e`.`age` as `age`, `e`.`fname` as `name` FROM `default` as `e` WHERE ((`e`.`age` > 10) AND (`e`.`fname` = 'Sam')) ORDER BY `e`.`age` ASC";

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


            const string expected = "SELECT `e`.`age` as `age` FROM `default` as `e` ORDER BY `e`.`age` ASC, `e`.`email` DESC";

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


            const string expected = "SELECT `e`.`age` as `age` FROM `default` as `e` ORDER BY `e`.`age` DESC, `e`.`email` ASC";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }
    }
}