using System.Linq;
using System.Threading.Tasks;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.UnitTests.Documents;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    public class AnyAllTests : N1QLTestBase
    {
        [TestCase(false)]
        [TestCase(true)]
        public async Task Any_NoPredicate(bool async)
        {
            // Arrange

            var query = CreateQueryable<Contact>("default");

            // Act

            _ = async ? await query.AnyAsync() : query.Any();
            var n1QlQuery = QueryExecutor.Query;

            // Assert

            const string expected = "SELECT true as result FROM `default` as `Extent1` LIMIT 1";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task Any_WithPredicate(bool async)
        {
            // Arrange

            var query = CreateQueryable<Contact>("default");

            // Act

            _ = async ? await query.AnyAsync(p => p.Age > 5) : query.Any(p => p.Age > 5);
            var n1QlQuery = QueryExecutor.Query;

            // Assert

            const string expected = "SELECT true as result FROM `default` as `Extent1` WHERE (`Extent1`.`age` > 5) LIMIT 1";

            Assert.AreEqual(expected, n1QlQuery);
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task All_WithPredicate(bool async)
        {
            // Arrange

            var query = CreateQueryable<Contact>("default");

            // Act

            _ = async ? await query.AllAsync(p => p.Age > 5) : query.All(p => p.Age > 5);
            var n1QlQuery = QueryExecutor.Query;

            // Assert

            const string expected = "SELECT false as result FROM `default` as `Extent1` WHERE NOT ((`Extent1`.`age` > 5)) LIMIT 1";

            Assert.AreEqual(expected, n1QlQuery);
        }
    }
}
