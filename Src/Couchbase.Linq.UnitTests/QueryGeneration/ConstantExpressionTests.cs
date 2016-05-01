using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    class ConstantExpressionTests : N1QLTestBase
    {

        [Test]
        public void Test_String()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.FirstName != "Test");

            const string expected =
                "SELECT `Extent1`.* FROM `default` as `Extent1` WHERE (`Extent1`.`fname` != 'Test')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_True()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => new { e.FirstName, Value = true });

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName`, TRUE as `Value` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_False()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => new { e.FirstName, Value = false });

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName`, FALSE as `Value` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Null()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Select(e => new { e.FirstName, Value = (string)null });

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName`, NULL as `Value` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_NewArrayOfConstants()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Select(e => new { e.FirstName, Value = new[] {1, 2, 3, 4, 5} });

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName`, [1, 2, 3, 4, 5] as `Value` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_NewArrayOfResults()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Select(e => new { Value = new[] { e.FirstName, e.LastName } });

            const string expected =
                "SELECT [`Extent1`.`fname`, `Extent1`.`lname`] as `Value` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_NewObject()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Select(e => new { Value = new { e.FirstName, e.LastName } });

            const string expected =
                "SELECT {\"FirstName\": `Extent1`.`fname`, \"LastName\": `Extent1`.`lname`} as `Value` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_NewObjectInArray()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Select(e => new {Value = new[] { new {Name = e.FirstName}, new {Name = e.LastName} } });

            const string expected =
                "SELECT [{\"Name\": `Extent1`.`fname`}, {\"Name\": `Extent1`.`lname`}] as `Value` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_DateTime()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Select(e => new { e.FirstName, Value = new DateTime(2000, 12, 1, 1, 23, 45, 67, DateTimeKind.Utc) });

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName`, \"2000-12-01T01:23:45.067Z\" as `Value` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Decimal()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                .Where(e => e.Abv == 0.5M);

            const string expected =
                "SELECT `Extent1`.* FROM `default` as `Extent1` WHERE (`Extent1`.`abv` = 0.5)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_DecimalInCommaCulture()
        {
            var currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            try
            {
                Assert.AreEqual(",", System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);

                var mockBucket = new Mock<IBucket>();
                mockBucket.SetupGet(e => e.Name).Returns("default");

                var query =
                    QueryFactory.Queryable<Beer>(mockBucket.Object)
                        .Where(e => e.Abv == 0.5M);

                const string expected =
                    "SELECT `Extent1`.* FROM `default` as `Extent1` WHERE (`Extent1`.`abv` = 0.5)";

                var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

                Assert.AreEqual(expected, n1QlQuery);
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            }
        }
    }
}
