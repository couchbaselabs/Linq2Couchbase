using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
    public class JoinTests : N1QLTestBase
    {
        [Test]
        public void Test_InnerJoin_Simple()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from beer in QueryFactory.Queryable<Beer>(mockBucket.Object)
                        join brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                        on beer.BreweryId equals N1Ql.Key(brewery)
                        select new {beer.Name, beer.Abv, BreweryName = brewery.Name};

            const string expected = "SELECT `Extent1`.`name` as `Name`, `Extent1`.`abv` as `Abv`, `Extent2`.`name` as `BreweryName` " +
                "FROM `default` as `Extent1` "+
                "INNER JOIN `default` as `Extent2` " +
                "ON KEYS `Extent1`.`brewery_id`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_InnerJoin_SortAndFilter()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from beer in QueryFactory.Queryable<Beer>(mockBucket.Object)
                        join brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                        on beer.BreweryId equals N1Ql.Key(brewery)
                        where brewery.Geo.Longitude > -80
                        orderby beer.Name
                        select new { beer.Name, beer.Abv, BreweryName = brewery.Name };

            const string expected = "SELECT `Extent1`.`name` as `Name`, `Extent1`.`abv` as `Abv`, `Extent2`.`name` as `BreweryName` " +
                "FROM `default` as `Extent1` " +
                "INNER JOIN `default` as `Extent2` " +
                "ON KEYS `Extent1`.`brewery_id` " +
                "WHERE (`Extent2`.`geo`.`lon` > -80) " + 
                "ORDER BY `Extent1`.`name` ASC";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_InnerJoin_Prefiltered()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from beer in QueryFactory.Queryable<Beer>(mockBucket.Object).Where(p => p.Type == "beer")
                        join brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object).Where(p => p.Type == "brewery")
                        on beer.BreweryId equals N1Ql.Key(brewery)
                        select new { beer.Name, beer.Abv, BreweryName = brewery.Name };

            const string expected = "SELECT `Extent1`.`name` as `Name`, `Extent1`.`abv` as `Abv`, `Extent2`.`name` as `BreweryName` " +
                "FROM `default` as `Extent1` " +
                "INNER JOIN `default` as `Extent2` " +
                "ON KEYS `Extent1`.`brewery_id` " + 
                "WHERE (`Extent1`.`type` = 'beer') AND (`Extent2`.`type` = 'brewery')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_LeftJoin_Simple()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from beer in QueryFactory.Queryable<Beer>(mockBucket.Object)
                        join breweryGroup in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                        on beer.BreweryId equals N1Ql.Key(breweryGroup) into bg
                        from brewery in bg.DefaultIfEmpty()
                        select new { beer.Name, beer.Abv, BreweryName = brewery.Name };

            const string expected = "SELECT `Extent1`.`name` as `Name`, `Extent1`.`abv` as `Abv`, `Extent2`.`name` as `BreweryName` " +
                "FROM `default` as `Extent1` " +
                "LEFT JOIN `default` as `Extent2` " +
                "ON KEYS `Extent1`.`brewery_id`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_LeftJoin_SortAndFilter()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from beer in QueryFactory.Queryable<Beer>(mockBucket.Object)
                        join breweryGroup in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                        on beer.BreweryId equals N1Ql.Key(breweryGroup) into bg
                        from brewery in bg.DefaultIfEmpty()
                        where beer.Abv > 4
                        orderby brewery.Name, beer.Name
                        select new { beer.Name, beer.Abv, BreweryName = brewery.Name };

            const string expected = "SELECT `Extent1`.`name` as `Name`, `Extent1`.`abv` as `Abv`, `Extent2`.`name` as `BreweryName` " +
                "FROM `default` as `Extent1` " +
                "LEFT JOIN `default` as `Extent2` " +
                "ON KEYS `Extent1`.`brewery_id` " +
                "WHERE (`Extent1`.`abv` > 4) " +
                "ORDER BY `Extent2`.`name` ASC, `Extent1`.`name` ASC";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_LeftJoin_Prefiltered()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from beer in QueryFactory.Queryable<Beer>(mockBucket.Object).Where(p => p.Type == "beer")
                        join breweryGroup in QueryFactory.Queryable<Brewery>(mockBucket.Object).Where(p => p.Type == "brewery")
                        on beer.BreweryId equals N1Ql.Key(breweryGroup) into bg
                        from brewery in bg.DefaultIfEmpty()
                        select new { beer.Name, beer.Abv, BreweryName = brewery.Name };

            const string expected = "SELECT `Extent1`.`name` as `Name`, `Extent1`.`abv` as `Abv`, `Extent2`.`name` as `BreweryName` " +
                "FROM `default` as `Extent1` " +
                "LEFT JOIN `default` as `Extent2` " +
                "ON KEYS `Extent1`.`brewery_id` " +
                "WHERE (`Extent1`.`type` = 'beer') AND (`Extent2`.`type` = 'brewery')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }
    }
}
