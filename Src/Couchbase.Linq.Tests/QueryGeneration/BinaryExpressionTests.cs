using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Linq.Tests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.Tests.QueryGeneration
{
    [TestFixture]
    class BinaryExpressionTests : N1QLTestBase
    {

        #region Logical Operators

        [Test]
        public void Test_BooleanAnd()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Age == 10 && e.FirstName != null)
                    .Select(e => new { age = e.Age });

            const string expected =
                "SELECT `Extent1`.`age` as `age` FROM `default` as `Extent1` WHERE ((`Extent1`.`age` = 10) AND (`Extent1`.`fname` IS NOT NULL))";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_BooleanOr()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Age == 10 || e.FirstName != null)
                    .Select(e => new { age = e.Age });

            const string expected =
                "SELECT `Extent1`.`age` as `age` FROM `default` as `Extent1` WHERE ((`Extent1`.`age` = 10) OR (`Extent1`.`fname` IS NOT NULL))";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        #region Comparison Operators

        [Test]
        public void Test_Equal()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Age == 10)
                    .Select(e => new { age = e.Age });

            const string expected =
                "SELECT `Extent1`.`age` as `age` FROM `default` as `Extent1` WHERE (`Extent1`.`age` = 10)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_NotEqual()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Age != 10)
                    .Select(e => new { age = e.Age });

            const string expected =
                "SELECT `Extent1`.`age` as `age` FROM `default` as `Extent1` WHERE (`Extent1`.`age` != 10)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IsNull()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.FirstName == null)
                    .Select(e => new { firstName = e.FirstName });

            const string expected =
                "SELECT `Extent1`.`fname` as `firstName` FROM `default` as `Extent1` WHERE (`Extent1`.`fname` IS NULL)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IsNotNull()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.FirstName != null)
                    .Select(e => new { firstName = e.FirstName });

            const string expected =
                "SELECT `Extent1`.`fname` as `firstName` FROM `default` as `Extent1` WHERE (`Extent1`.`fname` IS NOT NULL)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_GreaterThan()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Age > 10)
                    .Select(e => new { age = e.Age });

            const string expected =
                "SELECT `Extent1`.`age` as `age` FROM `default` as `Extent1` WHERE (`Extent1`.`age` > 10)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_GreaterThanOrEqual()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Age >= 10)
                    .Select(e => new { age = e.Age });

            const string expected =
                "SELECT `Extent1`.`age` as `age` FROM `default` as `Extent1` WHERE (`Extent1`.`age` >= 10)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_LessThan()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Age < 10)
                    .Select(e => new { age = e.Age });

            const string expected =
                "SELECT `Extent1`.`age` as `age` FROM `default` as `Extent1` WHERE (`Extent1`.`age` < 10)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_LessThanOrEqual()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Age <= 10)
                    .Select(e => new { age = e.Age });

            const string expected =
                "SELECT `Extent1`.`age` as `age` FROM `default` as `Extent1` WHERE (`Extent1`.`age` <= 10)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        #region Arithmetic Operators

        [Test]
        public void Test_Addition()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => new { age = e.Age + 2});

            const string expected =
                "SELECT (`Extent1`.`age` + 2) as `age` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Subtraction()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => new { age = e.Age - 2 });

            const string expected =
                "SELECT (`Extent1`.`age` - 2) as `age` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Multiplication()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => new { age = e.Age * 2 });

            const string expected =
                "SELECT (`Extent1`.`age` * 2) as `age` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Division()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => new { age = e.Age / 2 });

            const string expected =
                "SELECT (`Extent1`.`age` / 2) as `age` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Modulus()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => new { age = e.Age % 2 });

            const string expected =
                "SELECT (`Extent1`.`age` % 2) as `age` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        #region String Operators

        [Test]
        public void Test_StringAddition()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => new { name = e.FirstName + " " + e.LastName });

            const string expected =
                "SELECT ((`Extent1`.`fname` || ' ') || `Extent1`.`lname`) as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringConcat()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => new { name = String.Concat(e.FirstName, " ", e.LastName) });

            const string expected =
                "SELECT (`Extent1`.`fname` || ' ' || `Extent1`.`lname`) as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringConcatArray()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => new { name = String.Concat(new[] {e.FirstName, " ", e.LastName}) });

            const string expected =
                "SELECT (`Extent1`.`fname` || ' ' || `Extent1`.`lname`) as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringConcatFiveParameters()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => new { name = String.Concat(e.FirstName, " ", e.LastName, " ", "suffix") });

            const string expected =
                "SELECT (`Extent1`.`fname` || ' ' || `Extent1`.`lname` || ' ' || 'suffix') as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        #region "Coalesce Operators"

        [Test]
        public void Test_Coalesce_Single()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => new { name = e.FirstName ?? e.LastName });

            const string expected =
                "SELECT IFMISSINGORNULL(`Extent1`.`fname`, `Extent1`.`lname`) as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Coalesce_Double()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => new { name = e.FirstName ?? e.LastName ?? e.Email });

            const string expected =
                "SELECT IFMISSINGORNULL(`Extent1`.`fname`, `Extent1`.`lname`, `Extent1`.`email`) as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

    }
}
