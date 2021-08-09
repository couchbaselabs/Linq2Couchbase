using System;
using System.Collections.Generic;
using System.Linq;
using Couchbase.Core;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    public class UnionTests : N1QLTestBase
    {
        [Test]
        public void Test_Union()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .Where(e => e.Type == "beer")
                    .Select(e => new { e.Name })
                    .Union(
                        QueryFactory.Queryable<Brewery>(mockBucket.Object)
                            .Where(e => e.Type == "brewery")
                            .Select(e => new { e.Name }));

            const string expected =
                "SELECT `Extent1`.`name` as `Name` FROM `default` as `Extent1` WHERE (`Extent1`.`type` = \"beer\")" +
                " UNION " +
                "SELECT `Extent2`.`name` as `Name` FROM `default` as `Extent2` WHERE (`Extent2`.`type` = \"brewery\")";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_SortedUnion()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .Where(e => e.Type == "beer")
                    .Select(e => new { e.Name })
                    .Union(
                        QueryFactory.Queryable<Brewery>(mockBucket.Object)
                            .Where(e => e.Type == "brewery")
                            .Select(e => new { e.Name }))
                    .OrderBy(e => e.Name);

            const string expected =
                "SELECT `Extent1`.`name` as `Name` FROM `default` as `Extent1` WHERE (`Extent1`.`type` = \"beer\")" +
                " UNION " +
                "SELECT `Extent2`.`name` as `Name` FROM `default` as `Extent2` WHERE (`Extent2`.`type` = \"brewery\")" +
                " ORDER BY `Name` ASC";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_MultiUnionType1()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .Where(e => e.Type == "beer")
                    .Select(e => new {e.Name})
                    .Union(
                        QueryFactory.Queryable<Brewery>(mockBucket.Object)
                            .Where(e => e.Type == "brewery")
                            .Select(e => new {e.Name})
                            .Union(
                                QueryFactory.Queryable<Contact>(mockBucket.Object)
                                    .Where(e => e.Type == "contact")
                                    .Select(e => new { Name = e.FirstName})));

            const string expected =
                "SELECT `Extent1`.`name` as `Name` FROM `default` as `Extent1` WHERE (`Extent1`.`type` = \"beer\")" +
                " UNION " +
                "SELECT `Extent2`.`name` as `Name` FROM `default` as `Extent2` WHERE (`Extent2`.`type` = \"brewery\")" +
                " UNION " +
                "SELECT `Extent3`.`fname` as `Name` FROM `default` as `Extent3` WHERE (`Extent3`.`type` = \"contact\")";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_MultiUnionType2()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .Where(e => e.Type == "beer")
                    .Select(e => new { e.Name })
                    .Union(
                        QueryFactory.Queryable<Brewery>(mockBucket.Object)
                            .Where(e => e.Type == "brewery")
                            .Select(e => new { e.Name }))
                    .Union(
                        QueryFactory.Queryable<Contact>(mockBucket.Object)
                            .Where(e => e.Type == "contact")
                            .Select(e => new { Name = e.FirstName }));

            const string expected =
                "SELECT `Extent1`.`name` as `Name` FROM `default` as `Extent1` WHERE (`Extent1`.`type` = \"beer\")" +
                " UNION " +
                "SELECT `Extent2`.`name` as `Name` FROM `default` as `Extent2` WHERE (`Extent2`.`type` = \"brewery\")" +
                " UNION " +
                "SELECT `Extent3`.`fname` as `Name` FROM `default` as `Extent3` WHERE (`Extent3`.`type` = \"contact\")";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Concat()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .Where(e => e.Type == "beer")
                    .Select(e => new { e.Name })
                    .Concat(
                        QueryFactory.Queryable<Brewery>(mockBucket.Object)
                            .Where(e => e.Type == "brewery")
                            .Select(e => new { e.Name }));

            const string expected =
                "SELECT `Extent1`.`name` as `Name` FROM `default` as `Extent1` WHERE (`Extent1`.`type` = \"beer\")" +
                " UNION ALL " +
                "SELECT `Extent2`.`name` as `Name` FROM `default` as `Extent2` WHERE (`Extent2`.`type` = \"brewery\")";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_SortedConcat()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .Where(e => e.Type == "beer")
                    .Select(e => new { e.Name })
                    .Concat(
                        QueryFactory.Queryable<Brewery>(mockBucket.Object)
                            .Where(e => e.Type == "brewery")
                            .Select(e => new { e.Name }))
                    .OrderBy(e => e.Name);

            const string expected =
                "SELECT `Extent1`.`name` as `Name` FROM `default` as `Extent1` WHERE (`Extent1`.`type` = \"beer\")" +
                " UNION ALL " +
                "SELECT `Extent2`.`name` as `Name` FROM `default` as `Extent2` WHERE (`Extent2`.`type` = \"brewery\")" +
                " ORDER BY `Name` ASC";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_MultiConcatType1()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .Where(e => e.Type == "beer")
                    .Select(e => new { e.Name })
                    .Concat(
                        QueryFactory.Queryable<Brewery>(mockBucket.Object)
                            .Where(e => e.Type == "brewery")
                            .Select(e => new { e.Name })
                            .Concat(
                                QueryFactory.Queryable<Contact>(mockBucket.Object)
                                    .Where(e => e.Type == "contact")
                                    .Select(e => new { Name = e.FirstName })));

            const string expected =
                "SELECT `Extent1`.`name` as `Name` FROM `default` as `Extent1` WHERE (`Extent1`.`type` = \"beer\")" +
                " UNION ALL " +
                "SELECT `Extent2`.`name` as `Name` FROM `default` as `Extent2` WHERE (`Extent2`.`type` = \"brewery\")" +
                " UNION ALL " +
                "SELECT `Extent3`.`fname` as `Name` FROM `default` as `Extent3` WHERE (`Extent3`.`type` = \"contact\")";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_MultiConcatType2()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .Where(e => e.Type == "beer")
                    .Select(e => new { e.Name })
                    .Concat(
                        QueryFactory.Queryable<Brewery>(mockBucket.Object)
                            .Where(e => e.Type == "brewery")
                            .Select(e => new { e.Name }))
                    .Concat(
                        QueryFactory.Queryable<Contact>(mockBucket.Object)
                            .Where(e => e.Type == "contact")
                            .Select(e => new { Name = e.FirstName }));

            const string expected =
                "SELECT `Extent1`.`name` as `Name` FROM `default` as `Extent1` WHERE (`Extent1`.`type` = \"beer\")" +
                " UNION ALL " +
                "SELECT `Extent2`.`name` as `Name` FROM `default` as `Extent2` WHERE (`Extent2`.`type` = \"brewery\")" +
                " UNION ALL " +
                "SELECT `Extent3`.`fname` as `Name` FROM `default` as `Extent3` WHERE (`Extent3`.`type` = \"contact\")";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }
    }
}