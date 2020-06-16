using System.Linq;
using System.Threading.Tasks;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.UnitTests.Documents;
using Couchbase.Linq.Versioning;
using Moq;
using NUnit.Framework;

// ReSharper disable StringCompareIsCultureSpecific.1
// ReSharper disable StringCompareToIsCultureSpecific
// ReSharper disable StringIndexOfIsCultureSpecific.1
namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    class AggregateTests : N1QLTestBase
    {

        #region Simple Aggregates

        [Test]
        public void Test_Avg()
        {
            // ReSharper disable once UnusedVariable
            _ = CreateQueryable<Beer>("default").Average(p => p.Abv);
            var n1QlQuery = QueryExecutor.Query;

            const string expected =
                "SELECT AVG(`Extent1`.`abv`) as `result` FROM `default` as `Extent1`";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Avg_Raw()
        {
            var queryExecutor = new ClusterQueryExecutorEmulator(this, FeatureVersions.SelectRaw);

            // ReSharper disable once UnusedVariable
            _ = CreateQueryable<Beer>("default", queryExecutor).Average(p => p.Abv);
            var n1QlQuery = queryExecutor.Query;

            const string expected =
                "SELECT RAW AVG(`Extent1`.`abv`) FROM `default` as `Extent1`";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public async Task Test_AvgAsync()
        {
            // ReSharper disable once UnusedVariable
            _ = await CreateQueryable<Beer>("default").AverageAsync(p => p.Abv);
            var n1QlQuery = QueryExecutor.Query;

            const string expected =
                "SELECT AVG(`Extent1`.`abv`) as `result` FROM `default` as `Extent1`";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public async Task Test_AvgAsync_NoSelector()
        {
            // ReSharper disable once UnusedVariable
            _ = await CreateQueryable<Beer>("default").Select(p => p.Abv).AverageAsync();
            var n1QlQuery = QueryExecutor.Query;

            const string expected =
                "SELECT AVG(`Extent1`.`abv`) as `result` FROM `default` as `Extent1`";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public async Task Test_AvgAsync_Raw()
        {
            var queryExecutor = new ClusterQueryExecutorEmulator(this, FeatureVersions.SelectRaw);

            // ReSharper disable once UnusedVariable
            _ = await CreateQueryable<Beer>("default", queryExecutor).AverageAsync(p => p.Abv);
            var n1QlQuery = queryExecutor.Query;

            const string expected =
                "SELECT RAW AVG(`Extent1`.`abv`) FROM `default` as `Extent1`";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Count()
        {
            // ReSharper disable once UnusedVariable
            _ = CreateQueryable<Beer>("default").Count();
            var n1QlQuery = QueryExecutor.Query;

            const string expected =
                "SELECT COUNT(*) as `result` FROM `default` as `Extent1`";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public async Task Test_CountAsync()
        {
            // ReSharper disable once UnusedVariable
            _ = await CreateQueryable<Beer>("default").CountAsync();
            var n1QlQuery = QueryExecutor.Query;

            const string expected =
                "SELECT COUNT(*) as `result` FROM `default` as `Extent1`";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public async Task Test_CountAsync_WithPredicate()
        {
            // ReSharper disable once UnusedVariable
            _ = await CreateQueryable<Beer>("default").CountAsync(p => p.Abv == 1);
            var n1QlQuery = QueryExecutor.Query;

            const string expected =
                "SELECT COUNT(*) as `result` FROM `default` as `Extent1` WHERE (`Extent1`.`abv` = 1)";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Count_Raw()
        {
            var queryExecutor = new ClusterQueryExecutorEmulator(this, FeatureVersions.SelectRaw);

            // ReSharper disable once UnusedVariable
            _ = CreateQueryable<Beer>("default", queryExecutor).Count();
            var n1QlQuery = queryExecutor.Query;

            const string expected =
                "SELECT RAW COUNT(*) FROM `default` as `Extent1`";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_CountAfterSelectProjection()
        {
            // ReSharper disable once UnusedVariable
            _ = CreateQueryable<Beer>("default")
                .Select(p => new { p.Name, p.Description})
                .Count();
            var n1QlQuery = QueryExecutor.Query;

            const string expected =
                "SELECT COUNT({\"Name\": `Extent1`.`name`, \"Description\": `Extent1`.`description`}) as `result` FROM `default` as `Extent1`";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_CountAfterSelectProjection_Raw()
        {
            var queryExecutor = new ClusterQueryExecutorEmulator(this, FeatureVersions.SelectRaw);

            // ReSharper disable once UnusedVariable
            _ = CreateQueryable<Beer>("default", queryExecutor)
                .Select(p => new { p.Name, p.Description })
                .Count();
            var n1QlQuery = queryExecutor.Query;

            const string expected =
                "SELECT RAW COUNT({\"Name\": `Extent1`.`name`, \"Description\": `Extent1`.`description`}) FROM `default` as `Extent1`";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_CountProperty()
        {
            // ReSharper disable once UnusedVariable
            _ = CreateQueryable<Beer>("default").Select(p => p.Name).Count();
            var n1QlQuery = QueryExecutor.Query;

            const string expected =
                "SELECT COUNT(`Extent1`.`name`) as `result` FROM `default` as `Extent1`";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_CountDistinct()
        {
            // ReSharper disable once UnusedVariable
            _ = CreateQueryable<Beer>("default").Select(p => p.Name).Distinct().Count();
            var n1QlQuery = QueryExecutor.Query;

            const string expected =
                "SELECT COUNT(DISTINCT `Extent1`.`name`) as `result` FROM `default` as `Extent1`";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_LongCount()
        {
            // ReSharper disable once UnusedVariable
            _ = CreateQueryable<Beer>("default").LongCount();
            var n1QlQuery = QueryExecutor.Query;

            const string expected =
                "SELECT COUNT(*) as `result` FROM `default` as `Extent1`";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public async Task Test_LongCountAsync()
        {
            // ReSharper disable once UnusedVariable
            _ = await CreateQueryable<Beer>("default").LongCountAsync();
            var n1QlQuery = QueryExecutor.Query;

            const string expected =
                "SELECT COUNT(*) as `result` FROM `default` as `Extent1`";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public async Task Test_LongCountAsync_WithPredicate()
        {
            // ReSharper disable once UnusedVariable
            _ = await CreateQueryable<Beer>("default").LongCountAsync(p => p.Abv == 1);
            var n1QlQuery = QueryExecutor.Query;

            const string expected =
                "SELECT COUNT(*) as `result` FROM `default` as `Extent1` WHERE (`Extent1`.`abv` = 1)";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Min()
        {
            // ReSharper disable once UnusedVariable
            _ = CreateQueryable<Beer>("default").Min(p => p.Abv);
            var n1QlQuery = QueryExecutor.Query;

            const string expected =
                "SELECT MIN(`Extent1`.`abv`) as `result` FROM `default` as `Extent1`";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Max()
        {
            // ReSharper disable once UnusedVariable
            _ = CreateQueryable<Beer>("default").Max(p => p.Abv);
            var n1QlQuery = QueryExecutor.Query;

            const string expected =
                "SELECT MAX(`Extent1`.`abv`) as `result` FROM `default` as `Extent1`";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Sum()
        {
            // ReSharper disable once UnusedVariable
            _ = CreateQueryable<Beer>("default").Sum(p => p.Abv);
            var n1QlQuery = QueryExecutor.Query;

            const string expected =
                "SELECT SUM(`Extent1`.`abv`) as `result` FROM `default` as `Extent1`";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public async Task Test_SumAsync_NoSelector()
        {
            // ReSharper disable once UnusedVariable
            _ = await CreateQueryable<Beer>("default").Select(p => p.Abv).SumAsync();
            var n1QlQuery = QueryExecutor.Query;

            const string expected =
                "SELECT SUM(`Extent1`.`abv`) as `result` FROM `default` as `Extent1`";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public async Task Test_SumAsync_WithSelector()
        {
            // ReSharper disable once UnusedVariable
            _ = await CreateQueryable<Beer>("default").SumAsync(p => p.Abv);
            var n1QlQuery = QueryExecutor.Query;

            const string expected =
                "SELECT SUM(`Extent1`.`abv`) as `result` FROM `default` as `Extent1`";

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        #region "Group By"

        [Test]
        public void Test_SimpleGroupBy()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                from beer in QueryFactory.Queryable<Beer>(mockBucket.Object)
                group beer by beer.BreweryId
                into g
                select g.Key;

            const string expected =
                "SELECT `Extent1`.`brewery_id` as `result` " +
                "FROM `default` as `Extent1` " +
                "GROUP BY `Extent1`.`brewery_id`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_MultiPartGroupBy()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                from beer in QueryFactory.Queryable<Beer>(mockBucket.Object)
                group beer by new {beer.BreweryId, beer.Category}
                into g
                select new {g.Key.BreweryId, g.Key.Category};

            const string expected =
                "SELECT `Extent1`.`brewery_id` as `BreweryId`, `Extent1`.`category` as `Category` " +
                "FROM `default` as `Extent1` " +
                "GROUP BY `Extent1`.`brewery_id`, `Extent1`.`category`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_SimpleGroupByWithAggregate()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                from beer in QueryFactory.Queryable<Beer>(mockBucket.Object)
                group beer by beer.BreweryId
                into g
                select new {breweryId = g.Key, avgAbv = g.Average(p => p.Abv)};

            const string expected =
                "SELECT `Extent1`.`brewery_id` as `breweryId`, AVG(`Extent1`.`abv`) as `avgAbv` " +
                "FROM `default` as `Extent1` " +
                "GROUP BY `Extent1`.`brewery_id`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_MultiPartGroupByWithAggregate()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                from beer in QueryFactory.Queryable<Beer>(mockBucket.Object)
                group beer by new { beer.BreweryId, beer.Category }
                into g
                select new { g.Key.BreweryId, g.Key.Category, avgAbv = g.Average(p => p.Abv) };

            const string expected =
                "SELECT `Extent1`.`brewery_id` as `BreweryId`, `Extent1`.`category` as `Category`, AVG(`Extent1`.`abv`) as `avgAbv` " +
                "FROM `default` as `Extent1` " +
                "GROUP BY `Extent1`.`brewery_id`, `Extent1`.`category`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_GroupByAfterJoin()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                from beer in QueryFactory.Queryable<Beer>(mockBucket.Object)
                join brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object) on beer.BreweryId equals
                    N1QlFunctions.Key(brewery)
                group beer by brewery.Name
                into g
                select new {breweryName = g.Key, avgAbv = g.Average(p => p.Abv)};

            const string expected =
                "SELECT `Extent2`.`name` as `breweryName`, AVG(`Extent1`.`abv`) as `avgAbv` " +
                "FROM `default` as `Extent1` " +
                "INNER JOIN `default` as `Extent2` ON KEYS `Extent1`.`brewery_id` " +
                "GROUP BY `Extent2`.`name`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        #region Group Ordering

        [Test]
        public void Test_OrderByKey()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                from beer in QueryFactory.Queryable<Beer>(mockBucket.Object)
                join brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object) on beer.BreweryId equals N1QlFunctions.Key(brewery)
                group beer by brewery.Name
                into g
                orderby g.Key
                select new { breweryName = g.Key, avgAbv = g.Average(p => p.Abv) };

            const string expected =
                "SELECT `Extent2`.`name` as `breweryName`, AVG(`Extent1`.`abv`) as `avgAbv` " +
                "FROM `default` as `Extent1` " +
                "INNER JOIN `default` as `Extent2` ON KEYS `Extent1`.`brewery_id` " +
                "GROUP BY `Extent2`.`name` " +
                "ORDER BY `Extent2`.`name` ASC";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_OrderByAggregate()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                from beer in QueryFactory.Queryable<Beer>(mockBucket.Object)
                join brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object) on beer.BreweryId equals N1QlFunctions.Key(brewery)
                group beer by brewery.Name
                into g
                orderby g.Average(p => p.Abv) descending
                select new { breweryName = g.Key, avgAbv = g.Average(p => p.Abv) };

            const string expected =
                "SELECT `Extent2`.`name` as `breweryName`, AVG(`Extent1`.`abv`) as `avgAbv` " +
                "FROM `default` as `Extent1` " +
                "INNER JOIN `default` as `Extent2` ON KEYS `Extent1`.`brewery_id` " +
                "GROUP BY `Extent2`.`name` " +
                "ORDER BY AVG(`Extent1`.`abv`) DESC";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        #region Having

        [Test]
        public void Test_HavingByKey()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                from beer in QueryFactory.Queryable<Beer>(mockBucket.Object)
                join brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object) on beer.BreweryId equals N1QlFunctions.Key(brewery)
                group beer by brewery.Name
                into g
                where string.Compare(g.Key, "N") >= 0
                select new { breweryName = g.Key, avgAbv = g.Average(p => p.Abv) };

            const string expected =
                "SELECT `Extent2`.`name` as `breweryName`, AVG(`Extent1`.`abv`) as `avgAbv` " +
                "FROM `default` as `Extent1` " +
                "INNER JOIN `default` as `Extent2` ON KEYS `Extent1`.`brewery_id` " +
                "GROUP BY `Extent2`.`name` " +
                "HAVING (`Extent2`.`name` >= 'N')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_HavingByAggregate()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                from beer in QueryFactory.Queryable<Beer>(mockBucket.Object)
                join brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object) on beer.BreweryId equals N1QlFunctions.Key(brewery)
                group beer by brewery.Name
                into g
                where g.Average(p => p.Abv) >= 6
                select new { breweryName = g.Key, avgAbv = g.Average(p => p.Abv) };

            const string expected =
                "SELECT `Extent2`.`name` as `breweryName`, AVG(`Extent1`.`abv`) as `avgAbv` " +
                "FROM `default` as `Extent1` " +
                "INNER JOIN `default` as `Extent2` ON KEYS `Extent1`.`brewery_id` " +
                "GROUP BY `Extent2`.`name` " +
                "HAVING (AVG(`Extent1`.`abv`) >= 6)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        #region Array Aggregate

        [Test]
        public void Test_ArrayCount()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Route>(mockBucket.Object)
                .Where(p => p.Schedule.Count() > 1);

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression, FeatureVersions.ArrayInFromClause);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` " +
                "WHERE (ARRAY_LENGTH(`Extent1`.`schedule`) > 1)";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_ArrayCountFiltered()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Route>(mockBucket.Object)
                .Where(p => p.Schedule.Count(q => q.Day == 0) > 1);

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression, FeatureVersions.ArrayInFromClause);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` " +
                "WHERE (ARRAY_LENGTH(ARRAY `Extent2` FOR `Extent2` IN `Extent1`.`schedule` " +
                "WHEN (`Extent2`.`day` = 0) END) > 1)";

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion
    }
}
