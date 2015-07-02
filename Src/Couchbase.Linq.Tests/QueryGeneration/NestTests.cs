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

            const string expected = "SELECT brewery.name as name, address as address " +
                "FROM default as brewery "+
                "INNER UNNEST brewery.address as address";

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

            const string expected = "SELECT brewery.name as name, address as address " +
                "FROM default as brewery " +
                "INNER UNNEST brewery.address as address " +
                "ORDER BY address ASC";

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

            const string expected = "SELECT brewery.name as name, address as address " +
                "FROM default as brewery " +
                "INNER UNNEST brewery.address as address " +
                "WHERE (address != '123 First Street')";

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

            const string expected = "SELECT brewery.name as name, address as address " +
                "FROM default as brewery " +
                "OUTER UNNEST brewery.address as address";

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

            const string expected = "SELECT level3.Value as Value " +
                "FROM default as level1 " +
                "INNER UNNEST level1.Level2Items as level2 " +
                "INNER UNNEST level2.Level3Items as level3";

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

        #endregion

    }
}
