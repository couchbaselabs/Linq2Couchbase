using System;
using System.Linq;
using Couchbase.Core;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.UnitTests.Documents;
using Couchbase.Linq.Versioning;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    public class HintTests : N1QLTestBase
    {
        [Test]
        public void Test_UseIndex()
        {
            // ReSharper disable once UnusedVariable
            var temp = CreateQueryable<Contact>("default")
                    .UseIndex("IndexName")
                    .Where(e => e.Type == "contact")
                    .ToArray();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT RAW `Extent1` " +
                "FROM `default` as `Extent1` USE INDEX (`IndexName` USING GSI) " +
                "WHERE (`Extent1`.`type` = \"contact\")";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_UseIndex_MultipleClausesNotSupported()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .UseIndex("IndexName")
                .UseIndex("IndexName2")
                .Where(e => e.Type == "contact");

            Assert.Throws<NotSupportedException>(() => CreateN1QlQuery(mockBucket.Object, query.Expression));
        }

        [Test]
        public void Test_UseIndexWithType()
        {
            // ReSharper disable once UnusedVariable
            var temp = CreateQueryable<Contact>("default")
                    .UseIndex("IndexName", N1QlIndexType.View)
                    .Where(e => e.Type == "contact")
                    .ToArray();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT RAW `Extent1` " +
                "FROM `default` as `Extent1` USE INDEX (`IndexName` USING VIEW) " +
                "WHERE (`Extent1`.`type` = \"contact\")";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_UseIndex_Any()
        {
            // ReSharper disable once UnusedVariable
            var temp = CreateQueryable<Contact>("default")
                    .UseIndex("IndexName")
                    .Any(e => e.Type == "contact");
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT true as result " +
                "FROM `default` as `Extent1` USE INDEX (`IndexName` USING GSI) " +
                "WHERE (`Extent1`.`type` = \"contact\") " +
                "LIMIT 1";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_UseIndexWithType_Any()
        {
            // ReSharper disable once UnusedVariable
            var query = CreateQueryable<Contact>("default")
                    .UseIndex("IndexName", N1QlIndexType.View)
                    .Any(e => e.Type == "contact");
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT true as result " +
                "FROM `default` as `Extent1` USE INDEX (`IndexName` USING VIEW) " +
                "WHERE (`Extent1`.`type` = \"contact\") " +
                "LIMIT 1";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_UseIndexRightSideOfJoin()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from route in QueryFactory.Queryable<Route>(mockBucket.Object)
                join airport in QueryFactory.Queryable<Airport>(mockBucket.Object).UseIndex("IndexName")
                    on route.DestinationAirport equals airport.Faa
                select new {route, airport};

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            const string expected = "SELECT `Extent1` as `route`, `Extent2` as `airport` " +
                                    "FROM `default` as `Extent1` " +
                                    "INNER JOIN `default` as `Extent2` USE INDEX (`IndexName` USING GSI) " +
                                    "ON (`Extent1`.`destinationairport` = `Extent2`.`faa`)";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_UseIndexRightSideOfNest()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from airport in QueryFactory.Queryable<Airport>(mockBucket.Object)
                join route in QueryFactory.Queryable<Route>(mockBucket.Object).UseIndex("IndexName")
                    on airport.Faa equals route.DestinationAirport into routes
                select new {airport, routes};

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            const string expected = "SELECT `Extent1` as `airport`, `Extent2` as `routes` " +
                                    "FROM `default` as `Extent1` " +
                                    "LEFT OUTER NEST `default` as `Extent2` USE INDEX (`IndexName` USING GSI) " +
                                    "ON (`Extent1`.`faa` = `Extent2`.`destinationairport`)";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_UseIndexBothSidesOfJoin()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from route in QueryFactory.Queryable<Route>(mockBucket.Object).UseIndex("IndexName2")
                join airport in QueryFactory.Queryable<Airport>(mockBucket.Object).UseIndex("IndexName")
                    on route.DestinationAirport equals airport.Faa
                select new {route, airport};

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            const string expected = "SELECT `Extent1` as `route`, `Extent2` as `airport` " +
                                    "FROM `default` as `Extent1` USE INDEX (`IndexName2` USING GSI) " +
                                    "INNER JOIN `default` as `Extent2` USE INDEX (`IndexName` USING GSI) " +
                                    "ON (`Extent1`.`destinationairport` = `Extent2`.`faa`)";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_UseIndexBothSidesOfNest()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from airport in QueryFactory.Queryable<Airport>(mockBucket.Object).UseIndex("IndexName2")
                join route in QueryFactory.Queryable<Route>(mockBucket.Object).UseIndex("IndexName")
                    on airport.Faa equals route.DestinationAirport into routes
                select new {airport, routes};

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            const string expected = "SELECT `Extent1` as `airport`, `Extent2` as `routes` " +
                                    "FROM `default` as `Extent1` USE INDEX (`IndexName2` USING GSI) " +
                                    "LEFT OUTER NEST `default` as `Extent2` USE INDEX (`IndexName` USING GSI) " +
                                    "ON (`Extent1`.`faa` = `Extent2`.`destinationairport`)";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        [TestCase(HashHintType.Build, "build")]
        [TestCase(HashHintType.Probe, "probe")]
        public void Test_UseHash(HashHintType type, string typeString)
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from route in QueryFactory.Queryable<Route>(mockBucket.Object)
                join airport in QueryFactory.Queryable<Airport>(mockBucket.Object).UseHash(type)
                    on route.DestinationAirport equals airport.Faa
                select new {route, airport};

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            var expected =  "SELECT `Extent1` as `route`, `Extent2` as `airport` " +
                            "FROM `default` as `Extent1` " +
                            $"INNER JOIN `default` as `Extent2` USE HASH ({typeString}) " +
                            "ON (`Extent1`.`destinationairport` = `Extent2`.`faa`)";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        [TestCase(HashHintType.Build, "build")]
        [TestCase(HashHintType.Probe, "probe")]
        public void Test_UseHashNest(HashHintType type, string typeString)
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from airport in QueryFactory.Queryable<Airport>(mockBucket.Object)
                join route in QueryFactory.Queryable<Route>(mockBucket.Object).UseHash(type)
                    on airport.Faa equals route.DestinationAirport into routes
                select new {airport, routes};

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            var expected = "SELECT `Extent1` as `airport`, `Extent2` as `routes` " +
                           "FROM `default` as `Extent1` " +
                           $"LEFT OUTER NEST `default` as `Extent2` USE HASH ({typeString}) " +
                           "ON (`Extent1`.`faa` = `Extent2`.`destinationairport`)";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_UseHash_MainClauseNotSupported()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from route in QueryFactory.Queryable<Route>(mockBucket.Object).UseHash(HashHintType.Build)
                select route;

            Assert.Throws<NotSupportedException>(
                () => CreateN1QlQuery(mockBucket.Object, query.Expression));
        }

        [Test]
        public void Test_UseHash_MultipleClausesNotSupported()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from route in QueryFactory.Queryable<Route>(mockBucket.Object)
                    .UseHash(HashHintType.Build)
                    .UseHash(HashHintType.Probe)
                select route;

            Assert.Throws<NotSupportedException>(
                () => CreateN1QlQuery(mockBucket.Object, query.Expression));
        }

        [Test]
        public void Test_UseIndexAndHint()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from route in QueryFactory.Queryable<Route>(mockBucket.Object)
                join airport in QueryFactory.Queryable<Airport>(mockBucket.Object).UseIndex("IndexName").UseHash(HashHintType.Build)
                    on route.DestinationAirport equals airport.Faa
                select new {route, airport};

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            const string expected = "SELECT `Extent1` as `route`, `Extent2` as `airport` " +
                                    "FROM `default` as `Extent1` " +
                                    "INNER JOIN `default` as `Extent2` USE INDEX (`IndexName` USING GSI) HASH (build) " +
                                    "ON (`Extent1`.`destinationairport` = `Extent2`.`faa`)";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_UseIndexAndHintNest()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from airport in QueryFactory.Queryable<Airport>(mockBucket.Object)
                join route in QueryFactory.Queryable<Route>(mockBucket.Object).UseIndex("IndexName").UseHash(HashHintType.Build)
                    on airport.Faa equals route.DestinationAirport into routes
                select new {airport, routes};

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            const string expected = "SELECT `Extent1` as `airport`, `Extent2` as `routes` " +
                                    "FROM `default` as `Extent1` " +
                                    "LEFT OUTER NEST `default` as `Extent2` USE INDEX (`IndexName` USING GSI) HASH (build) " +
                                    "ON (`Extent1`.`faa` = `Extent2`.`destinationairport`)";

            Assert.AreEqual(expected, n1QlQuery);
        }
    }
}
