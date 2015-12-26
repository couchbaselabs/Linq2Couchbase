using System;
using System.Collections.Generic;
using System.Linq;
using Couchbase.Core;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    public class SubqueryTests : N1QLTestBase
    {
        [Test]
        public void Test_BucketSubqueryWithNewObjectSelection()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                        select new {
                            name = brewery.Name,
                            beers = QueryFactory.Queryable<Beer>(mockBucket.Object)
                                .UseKeys(brewery.Beers)
                                .Select(p => new { name = p.Name })
                                .ToArray()
                        };

            const string expected =
                "SELECT `Extent1`.`name` as `name`, " +
                "(SELECT `Extent2`.`name` as `name` FROM `default` as `Extent2` USE KEYS `Extent1`.`beers`) as `beers` " +
                "FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_BucketSubqueryWithStronglyTypedNewObjectSelection()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                        select new
                        {
                            name = brewery.Name,
                            beers = QueryFactory.Queryable<Beer>(mockBucket.Object)
                                .UseKeys(brewery.Beers)
                                .Select(p => new Beer() { Name = p.Name })
                                .ToArray()
                        };

            const string expected =
                "SELECT `Extent1`.`name` as `name`, " +
                "(SELECT `Extent2`.`name` as `name` FROM `default` as `Extent2` USE KEYS `Extent1`.`beers`) as `beers` " +
                "FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_BucketSubqueryWithPropertySelection()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                        select new
                        {
                            name = brewery.Name,
                            beers = QueryFactory.Queryable<Beer>(mockBucket.Object)
                                .UseKeys(brewery.Beers)
                                .Select(p => p.Name)
                                .ToArray()
                        };

            const string expected =
                "SELECT `Extent1`.`name` as `name`, " +
                "ARRAY `ArrayExtent`.`result` FOR `ArrayExtent` IN " +
                "(SELECT `Extent2`.`name` as `result` FROM `default` as `Extent2` USE KEYS `Extent1`.`beers`) END as `beers` " +
                "FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_BucketSubqueryWithNoSelection()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                        select new
                        {
                            name = brewery.Name,
                            beers = QueryFactory.Queryable<Beer>(mockBucket.Object)
                                .UseKeys(brewery.Beers)
                                .ToArray()
                        };

            const string expected =
                "SELECT `Extent1`.`name` as `name`, " +
                "(SELECT `Extent2`.* FROM `default` as `Extent2` USE KEYS `Extent1`.`beers`) as `beers` " +
                "FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_BucketSubqueryWithAnyFilter()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                        where QueryFactory.Queryable<Beer>(mockBucket.Object).UseKeys(brewery.Beers).Any(p => p.Name.Contains("IPA"))
                        select new
                        {
                            name = brewery.Name
                        };

            const string expected =
                "SELECT `Extent1`.`name` as `name` " +
                "FROM `default` as `Extent1` " +
                "WHERE ANY `Extent3` IN (SELECT * FROM `default` as `Extent2` USE KEYS `Extent1`.`beers` " +
                "WHERE (`Extent2`.`name` LIKE '%IPA%')) SATISFIES true END";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_BucketSubqueryWithAllFilter()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                        where QueryFactory.Queryable<Beer>(mockBucket.Object).UseKeys(brewery.Beers).All(p => p.Abv > 4)
                        select new
                        {
                            name = brewery.Name
                        };

            const string expected =
                "SELECT `Extent1`.`name` as `name` " +
                "FROM `default` as `Extent1` " +
                "WHERE EVERY `Extent3` IN (SELECT `Extent2` FROM `default` as `Extent2` USE KEYS `Extent1`.`beers`) " +
                "SATISFIES (`Extent3`.`Extent2`.`abv` > 4) END";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_ArraySubqueryWithFilter()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                from brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                select new {name = brewery.Name, addresses = brewery.Address.Where(p => p.Length > 3)};

            const string expected =
                "SELECT `Extent1`.`name` as `name`, " +
                "ARRAY `Extent2` FOR `Extent2` IN `Extent1`.`address` WHEN (LENGTH(`Extent2`) > 3) END as `addresses` " +
                "FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_ArraySubquerySelectNewObject()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                from brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                select new { name = brewery.Name, addresses = brewery.Address.Select(p => new { address = p }) };

            const string expected =
                "SELECT `Extent1`.`name` as `name`, " +
                "ARRAY {\"address\": `Extent2`} FOR `Extent2` IN `Extent1`.`address` END as `addresses` " +
                "FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_ArraySubquerySelectStronglyTypedNewObject()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                from brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                select new { name = brewery.Name, addresses = brewery.Address.Select(p => new Address() { AddressLine1 = p }) };

            const string expected =
                "SELECT `Extent1`.`name` as `name`, " +
                "ARRAY {\"address1\": `Extent2`} FOR `Extent2` IN `Extent1`.`address` END as `addresses` " +
                "FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_ArraySubquerySelectExpression()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                from brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                select new { name = brewery.Name, addresses = brewery.Address.Select(p => "Address " + p) };

            const string expected =
                "SELECT `Extent1`.`name` as `name`, " +
                "ARRAY ('Address ' || `Extent2`) FOR `Extent2` IN `Extent1`.`address` END as `addresses` " +
                "FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_ArraySubqueryWithSortAscending()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                from brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                select new { name = brewery.Name, addresses = brewery.Address.OrderBy(p => p) };

            const string expected =
                "SELECT `Extent1`.`name` as `name`, " +
                "ARRAY_SORT(`Extent1`.`address`) as `addresses` " +
                "FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_ArraySubqueryWithSortDescending()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                from brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                select new { name = brewery.Name, addresses = brewery.Address.OrderByDescending(p => p) };

            const string expected =
                "SELECT `Extent1`.`name` as `name`, " +
                "ARRAY_REVERSE(ARRAY_SORT(`Extent1`.`address`)) as `addresses` " +
                "FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_ArraySubqueryWithInvalidSort()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                from brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                select new { name = brewery.Name, addresses = brewery.Address.OrderByDescending(p => p.Length) };

            Assert.Throws<NotSupportedException>(() => CreateN1QlQuery(mockBucket.Object, query.Expression));
        }

        [Test]
        public void Test_ArraySubqueryAllWithPrefilter()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                from brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                where brewery.Address.Where(p => p == "563 Second Street").All(p => p == "101 Fake Street")
                select new { name = brewery.Name, addresses = brewery.Address };

            const string expected =
                "SELECT `Extent1`.`name` as `name`, `Extent1`.`address` as `addresses` " +
                "FROM `default` as `Extent1` " +
                "WHERE EVERY `Extent3` IN " +
                "(ARRAY `Extent2` FOR `Extent2` IN `Extent1`.`address` WHEN (`Extent2` = '563 Second Street') END) " +
                "SATISFIES (`Extent3` = '101 Fake Street') END";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }
    }
}