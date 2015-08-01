using System;
using System.Collections.Generic;
using System.Linq;
using Couchbase.Core;
using Couchbase.Linq.Tests.Documents;
using Moq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace Couchbase.Linq.Tests.QueryGeneration
{
    [TestFixture]
    public class ArrayIndexTests : N1QLTestBase
    {

        [Test]
        public void Test_ArrayIndexAccessor()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<DocumentWithArray>(mockBucket.Object)
                    .Select(e => new { address = e.Array[0] });

            const string expected = "SELECT `e`.`Array`[0] as `address` FROM `default` as `e`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_ListIndexAccessor()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Brewery>(mockBucket.Object)
                    .Select(e => new { address = e.Address[0] });

            const string expected = "SELECT `e`.`address`[0] as `address` FROM `default` as `e`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IListIndexAccessor()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<DocumentWithIList>(mockBucket.Object)
                    .Select(e => new { address = e.List[0] });

            const string expected = "SELECT `e`.`List`[0] as `address` FROM `default` as `e`";

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