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
    class UnaryExpressionTests : N1QLTestBase
    {

        #region Logical Operators

        [Test]
        public void Test_BooleanNot()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    // ReSharper disable once NegativeEqualityExpression
                    .Where(e => !(e.Age == 10))
                    .Select(e => new { age = e.Age });

            const string expected =
                "SELECT e.age as age FROM default as e WHERE NOT (e.age = 10)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        #region Arithmetic Operators

        [Test]
        public void Test_Negation()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => new {age = -e.Age});

            const string expected =
                "SELECT -e.age as age FROM default as e";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

    }
}
