using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    public class ExplainTests : N1QLTestBase
    {
        [Test]
        public void Test_Explain_Keyword()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<Contact>(mockBucket.Object)
                .Select(c => new {age = c.Age});

            var explainQuery = Expression.Call(null, 
                typeof(QueryExtensions).GetTypeInfo().GetMethod("Explain").MakeGenericMethod(query.ElementType), query.Expression);

            const string expected = "EXPLAIN SELECT `Extent1`.`age` as `age` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, explainQuery);

            Assert.AreEqual(expected, n1QlQuery);
        }
    }
}
