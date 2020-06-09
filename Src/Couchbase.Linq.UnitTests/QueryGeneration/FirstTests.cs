using System.Linq;
using System.Threading.Tasks;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.UnitTests.Documents;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    public class FirstTests : N1QLTestBase
    {
        [Test]
        public void Test_First()
        {
            var temp = CreateQueryable<Contact>("default").First();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` LIMIT 1";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_FirstOrDefault()
        {
            var temp = CreateQueryable<Contact>("default").FirstOrDefault();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` LIMIT 1";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_FirstWithSkip()
        {
            var temp = CreateQueryable<Contact>("default").Skip(10).First();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` LIMIT 1 OFFSET 10";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public async Task Test_FirstAsync()
        {
            var temp = await CreateQueryable<Contact>("default").FirstAsync();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` LIMIT 1";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public async Task Test_FirstAsyncWithPredicate()
        {
            var temp = await CreateQueryable<Contact>("default").FirstAsync(p => p.Age > 5);
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` WHERE (`Extent1`.`age` > 5) LIMIT 1";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public async Task Test_FirstOrDefaultAsync()
        {
            var temp = await CreateQueryable<Contact>("default").FirstOrDefaultAsync();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` LIMIT 1";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public async Task Test_FirstOrDefaultAsyncWithPredicate()
        {
            var temp = await CreateQueryable<Contact>("default").FirstOrDefaultAsync(p => p.Age > 5);
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` WHERE (`Extent1`.`age` > 5) LIMIT 1";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public async Task Test_FirstAsyncWithSkip()
        {
            var temp = await CreateQueryable<Contact>("default").Skip(10).FirstAsync();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` LIMIT 1 OFFSET 10";

            Assert.AreEqual(expected, n1QlQuery);
        }
    }
}
