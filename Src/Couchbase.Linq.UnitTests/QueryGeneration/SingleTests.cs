using System.Linq;
using System.Threading.Tasks;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.UnitTests.Documents;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    public class SingleTests : N1QLTestBase
    {
        [Test]
        public void Test_Single()
        {
            var temp = CreateQueryable<Contact>("default").Single();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` LIMIT 2";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_SingleOrDefault()
        {
            var temp = CreateQueryable<Contact>("default").SingleOrDefault();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` LIMIT 2";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_SingleWithSkip()
        {
            var temp = CreateQueryable<Contact>("default").Skip(10).Single();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` LIMIT 2 OFFSET 10";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public async Task Test_SingleAsync()
        {
            var temp = await CreateQueryable<Contact>("default").SingleAsync();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` LIMIT 2";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public async Task Test_SingleAsyncWithPredicate()
        {
            var temp = await CreateQueryable<Contact>("default").SingleAsync(p => p.Age > 5);
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` WHERE (`Extent1`.`age` > 5) LIMIT 2";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public async Task Test_SingleOrDefaultAsync()
        {
            var temp = await CreateQueryable<Contact>("default").SingleOrDefaultAsync();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` LIMIT 2";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public async Task Test_SingleOrDefaultAsyncWithPredicate()
        {
            var temp = await CreateQueryable<Contact>("default").SingleOrDefaultAsync(p => p.Age > 5);
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` WHERE (`Extent1`.`age` > 5) LIMIT 2";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public async Task Test_SingleAsyncWithSkip()
        {
            var temp = await CreateQueryable<Contact>("default").Skip(10).SingleAsync();
            var n1QlQuery = QueryExecutor.Query;

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` LIMIT 2 OFFSET 10";

            Assert.AreEqual(expected, n1QlQuery);
        }
    }
}
