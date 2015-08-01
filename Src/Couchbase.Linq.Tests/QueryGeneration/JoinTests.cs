using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Tests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.Tests.QueryGeneration
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

            const string expected = "SELECT `beer`.`name` as `Name`, `beer`.`abv` as `Abv`, `brewery`.`name` as `BreweryName` " +
                "FROM `default` as `beer` "+
                "INNER JOIN `default` as `brewery` " +
                "ON KEYS `beer`.`brewery_id`";

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

            const string expected = "SELECT `beer`.`name` as `Name`, `beer`.`abv` as `Abv`, `brewery`.`name` as `BreweryName` " +
                "FROM `default` as `beer` " +
                "INNER JOIN `default` as `brewery` " +
                "ON KEYS `beer`.`brewery_id` " +
                "WHERE (`brewery`.`geo`.`lon` > -80) " + 
                "ORDER BY `beer`.`name` ASC";

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

            const string expected = "SELECT `p`.`name` as `Name`, `p`.`abv` as `Abv`, `brewery`.`name` as `BreweryName` " +
                "FROM `default` as `p` " +
                "INNER JOIN `default` as `brewery` " +
                "ON KEYS `p`.`brewery_id` " + 
                "WHERE (`p`.`type` = 'beer') AND (`brewery`.`type` = 'brewery')";

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

            const string expected = "SELECT `beer`.`name` as `Name`, `beer`.`abv` as `Abv`, `brewery`.`name` as `BreweryName` " +
                "FROM `default` as `beer` " +
                "LEFT JOIN `default` as `brewery` " +
                "ON KEYS `beer`.`brewery_id`";

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

            const string expected = "SELECT `beer`.`name` as `Name`, `beer`.`abv` as `Abv`, `brewery`.`name` as `BreweryName` " +
                "FROM `default` as `beer` " +
                "LEFT JOIN `default` as `brewery` " +
                "ON KEYS `beer`.`brewery_id` " +
                "WHERE (`beer`.`abv` > 4) " +
                "ORDER BY `brewery`.`name` ASC, `beer`.`name` ASC";

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

            const string expected = "SELECT `p`.`name` as `Name`, `p`.`abv` as `Abv`, `brewery`.`name` as `BreweryName` " +
                "FROM `default` as `p` " +
                "LEFT JOIN `default` as `brewery` " +
                "ON KEYS `p`.`brewery_id` " +
                "WHERE (`p`.`type` = 'beer') AND (`brewery`.`type` = 'brewery')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }
    }
}
