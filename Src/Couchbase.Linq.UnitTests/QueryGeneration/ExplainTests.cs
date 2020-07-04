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
            var query = CreateQueryable<Contact>("default")
                .Select(c => new {age = c.Age});

            _ = query.Explain();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "EXPLAIN SELECT `Extent1`.`age` as `age` FROM `default` as `Extent1`";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public async Task Test_ExplainAsync_Keyword()
        {
            var query = CreateQueryable<Contact>("default")
                .Select(c => new {age = c.Age});

            _ = await query.ExplainAsync();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "EXPLAIN SELECT `Extent1`.`age` as `age` FROM `default` as `Extent1`";

            Assert.AreEqual(expected, n1QlQuery);
        }
    }
}
