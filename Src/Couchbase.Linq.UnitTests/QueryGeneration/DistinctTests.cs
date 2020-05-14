using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Core.Version;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    public class DistinctTests : N1QLTestBase
    {
        [Test]
        public void Test_Distinct_Keyword()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Select(c => new {age = c.Age})
                .Distinct();

            const string expected = "SELECT DISTINCT `Extent1`.`age` as `age` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_DistinctRaw_CorrectOrdering()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Select(c => c.Age)
                .Distinct();

            const string expected = "SELECT DISTINCT RAW `Extent1`.`age` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression,
                new ClusterVersion(new Version(5, 5)));

            Assert.AreEqual(expected, n1QlQuery);
        }
    }
}
