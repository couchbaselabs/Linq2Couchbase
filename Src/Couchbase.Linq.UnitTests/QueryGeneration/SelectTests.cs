using System.Linq;
using Couchbase.Core;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    public class SelectTests : N1QLTestBase
    {
        [Test]
        public void Test_Select_With_Projection()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => new {age = e.Age, name = e.FirstName});

            const string expected = "SELECT `Extent1`.`age` as `age`, `Extent1`.`fname` as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Select_WithStronglyTypedProjection()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => new Contact() { Age = e.Age, FirstName = e.FirstName });

            const string expected = "SELECT `Extent1`.`age` as `age`, `Extent1`.`fname` as `fname` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Select_All_Properties()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => e);

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Select_UseKeys()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .UseKeys(new[] { "abc", "def" })
                    .Select(e => e);

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` USE KEYS ['abc', 'def']";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Select_UseIndex()
        {
            // ReSharper disable once UnusedVariable
            var temp = CreateQueryable<Contact>("default")
                    .UseIndex("IndexName")
                    .Where(e => e.Type == "contact")
                    .ToArray();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* " +
                "FROM `default` as `Extent1` USE INDEX (`IndexName` USING GSI) " +
                "WHERE (`Extent1`.`type` = 'contact')";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Select_UseIndexWithType()
        {
            // ReSharper disable once UnusedVariable
            var temp = CreateQueryable<Contact>("default")
                    .UseIndex("IndexName", N1QlIndexType.View)
                    .Where(e => e.Type == "contact")
                    .ToArray();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* " +
                "FROM `default` as `Extent1` USE INDEX (`IndexName` USING VIEW) " +
                "WHERE (`Extent1`.`type` = 'contact')";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Select_UseIndex_Any()
        {
            // ReSharper disable once UnusedVariable
            var temp = CreateQueryable<Contact>("default")
                    .UseIndex("IndexName")
                    .Any(e => e.Type == "contact");
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT true as result " +
                "FROM `default` as `Extent1` USE INDEX (`IndexName` USING GSI) " +
                "WHERE (`Extent1`.`type` = 'contact') " +
                "LIMIT 1";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Select_UseIndexWithType_Any()
        {
            // ReSharper disable once UnusedVariable
            var query = CreateQueryable<Contact>("default")
                    .UseIndex("IndexName", N1QlIndexType.View)
                    .Any(e => e.Type == "contact");
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT true as result " +
                "FROM `default` as `Extent1` USE INDEX (`IndexName` USING VIEW) " +
                "WHERE (`Extent1`.`type` = 'contact') " +
                "LIMIT 1";

            Assert.AreEqual(expected, n1QlQuery);
        }
    }
}