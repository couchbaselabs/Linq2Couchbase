using System.Collections.Generic;
using System.Linq;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    public class ArrayOperatorTests : N1QLTestBase
    {
        [Test]
        public void Test_ArrayContains()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<DocumentWithArray>(mockBucket.Object)
                    .Where(e => e.Array.Contains("abc"));

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE \"abc\" IN (`Extent1`.`Array`)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_ListContains()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<DocumentWithIList>(mockBucket.Object)
                    .Where(e => e.List.Contains("abc"));

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE \"abc\" IN (`Extent1`.`List`)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StaticArrayContains()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var staticArray = new[] {"abc", "def"};
            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .Where(e => staticArray.Contains(e.Name));

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE `Extent1`.`name` IN ([\"abc\", \"def\"])";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_AlteredStaticArrayContains()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var staticArray = new[] { "abc", "def" };
            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .Where(e => staticArray.Select(p => "a" + p).Contains(e.Name));

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE `Extent1`.`name` IN (ARRAY (\"a\" || `Extent2`) FOR `Extent2` IN [\"abc\", \"def\"] END)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StaticListContains()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var staticArray = new List<string> { "abc", "def" };
            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .Where(e => staticArray.Contains(e.Name));

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE `Extent1`.`name` IN ([\"abc\", \"def\"])";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_ArrayAny()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Brewery>(mockBucket.Object)
                    .Where(e => e.Beers.Any(p => p == "test"));

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE ANY `Extent2` IN `Extent1`.`beers` SATISFIES (`Extent2` = \"test\") END";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringSplitAny()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .Where(e => e.Name.Split().Any(p => p == "test"));

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE ANY `Extent2` IN SPLIT(`Extent1`.`name`) SATISFIES (`Extent2` = \"test\") END";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringSplitTwoParamAny()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Beer>(mockBucket.Object)
                    .Where(e => e.Name.Split(new[] {' '}).Any(p => p == "test"));

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE ANY `Extent2` IN SPLIT(`Extent1`.`name`, \" \") SATISFIES (`Extent2` = \"test\") END";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #region Helper Classes

        // ReSharper disable once ClassNeverInstantiated.Local
        private class DocumentWithArray
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string[] Array { get; set; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class DocumentWithIList
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public IList<string> List { get; set; }
        }

        #endregion
    }
}