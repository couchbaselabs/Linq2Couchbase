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
    public class NestTests : N1QLTestBase
    {
        [Test]
        public void Test_Unnest_Simple()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                        from address in brewery.Address
                        select new {name = brewery.Name, address};

            const string expected = "SELECT `Extent1`.`name` as `name`, `Extent2` as `address` " +
                "FROM `default` as `Extent1` "+
                "INNER UNNEST `Extent1`.`address` as `Extent2`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Unnest_Sort()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                        from address in brewery.Address
                        orderby address
                        select new { name = brewery.Name, address };

            const string expected = "SELECT `Extent1`.`name` as `name`, `Extent2` as `address` " +
                "FROM `default` as `Extent1` " +
                "INNER UNNEST `Extent1`.`address` as `Extent2` " +
                "ORDER BY `Extent2` ASC";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Unnest_Prefiltered()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                        from address in brewery.Address.Where(p => p != "123 First Street")
                        select new { name = brewery.Name, address };

            const string expected = "SELECT `Extent1`.`name` as `name`, `Extent2` as `address` " +
                "FROM `default` as `Extent1` " +
                "INNER UNNEST `Extent1`.`address` as `Extent2` " +
                "WHERE (`Extent2` != '123 First Street')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_LeftUnnest_Simple()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from brewery in QueryFactory.Queryable<Brewery>(mockBucket.Object)
                        from address in brewery.Address.DefaultIfEmpty()
                        select new { name = brewery.Name, address };

            const string expected = "SELECT `Extent1`.`name` as `name`, `Extent2` as `address` " +
                "FROM `default` as `Extent1` " +
                "OUTER UNNEST `Extent1`.`address` as `Extent2`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Unnest_DoubleLevel()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from level1 in QueryFactory.Queryable<UnnestLevel1>(mockBucket.Object)
                        from level2 in level1.Level2Items
                        from level3 in level2.Level3Items
                        select new { level3.Value };

            const string expected = "SELECT `Extent3`.`Value` as `Value` " +
                "FROM `default` as `Extent1` " +
                "INNER UNNEST `Extent1`.`Level2Items` as `Extent2` " +
                "INNER UNNEST `Extent2`.`Level3Items` as `Extent3`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Nest_Simple()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<NestLevel1>(mockBucket.Object)
                .Nest(
                    QueryFactory.Queryable<NestLevel2>(mockBucket.Object),
                    level1 => level1.NestLevel2Keys,
                    (level1, level2) => new {level1.Value, level2});
                        
            const string expected = "SELECT `Extent1`.`Value` as `Value`, `Extent2` as `level2` " +
                "FROM `default` as `Extent1` " +
                "INNER NEST `default` as `Extent2` ON KEYS `Extent1`.`NestLevel2Keys`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Nest_Prefiltered()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<NestLevel1>(mockBucket.Object)
                .Where(level1 => level1.Type == "level1")
                .Nest(
                    QueryFactory.Queryable<NestLevel2>(mockBucket.Object).Where(level2 => level2.Type == "level2"),
                    level1 => level1.NestLevel2Keys,
                    (level1, level2) => new { level1.Value, level2 });

            const string expected = "SELECT `Extent1`.`Value` as `Value`, `Extent4` as `level2` " +
                "FROM `default` as `Extent1` " +
                "INNER NEST `default` as `Extent2` ON KEYS `Extent1`.`NestLevel2Keys` " +
                "LET `Extent4` = ARRAY `Extent3` FOR `Extent3` IN `Extent2` WHEN (`Extent3`.`Type` = 'level2') END " +
                "WHERE (`Extent1`.`Type` = 'level1') AND (ARRAY_LENGTH(`Extent4`) > 0)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_LeftOuterNest_Simple()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<NestLevel1>(mockBucket.Object)
                .LeftOuterNest(
                    QueryFactory.Queryable<NestLevel2>(mockBucket.Object),
                    level1 => level1.NestLevel2Keys,
                    (level1, level2) => new { level1.Value, level2 });

            const string expected = "SELECT `Extent1`.`Value` as `Value`, `Extent2` as `level2` " +
                "FROM `default` as `Extent1` " +
                "LEFT OUTER NEST `default` as `Extent2` ON KEYS `Extent1`.`NestLevel2Keys`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_LeftOuterNest_Prefiltered()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<NestLevel1>(mockBucket.Object)
                .Where(level1 => level1.Type == "level1")
                .LeftOuterNest(
                    QueryFactory.Queryable<NestLevel2>(mockBucket.Object).Where(level2 => level2.Type == "level2"),
                    level1 => level1.NestLevel2Keys,
                    (level1, level2) => new { level1.Value, level2 });

            const string expected = "SELECT `Extent1`.`Value` as `Value`, `Extent4` as `level2` " +
                "FROM `default` as `Extent1` " +
                "LEFT OUTER NEST `default` as `Extent2` ON KEYS `Extent1`.`NestLevel2Keys` " +
                "LET `Extent4` = ARRAY `Extent3` FOR `Extent3` IN `Extent2` WHEN (`Extent3`.`Type` = 'level2') END " +
                "WHERE (`Extent1`.`Type` = 'level1')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #region Helper Classes

        public class UnnestLevel1
        {
            public List<UnnestLevel2> Level2Items { get; set; }
        }

        public class UnnestLevel2
        {
            public List<UnnestLevel3> Level3Items {get; set;}
        }

        public class UnnestLevel3
        {
            public string Value { get; set; }
        }

        public class NestLevel1
        {
            public string Type { get; set; }
            public string Value { get; set; }
            public List<string> NestLevel2Keys { get; set; }
        }

        public class NestLevel2
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }

        #endregion

    }
}
