using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Linq.Tests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.Tests.QueryGeneration
{
    [TestFixture]
    class ConditionalExpressionTests : N1QLTestBase
    {

        [Test]
        public void Test_IfThenElse()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Select(e => new { e.FirstName, Value = e.Age < 10 ? null : e.LastName });

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName`, CASE WHEN (`Extent1`.`age` < 10) THEN NULL ELSE `Extent1`.`lname` END as `Value` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

    }
}
