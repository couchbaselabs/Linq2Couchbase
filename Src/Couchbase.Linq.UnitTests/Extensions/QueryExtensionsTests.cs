using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.Extensions;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.Extensions
{
    [TestFixture]
    public class QueryExtensionsTests
    {
        #region ExecuteAsync

        [Test(Description = "https://github.com/couchbaselabs/Linq2Couchbase/issues/191")]
        public async Task ExecuteAsync_EnumerableQuery_ReturnsResult()
        {
            // Arrange

            var query = Enumerable.Range(1, 100).AsQueryable();

            // Act

            var result = await query.Skip(5).Take(1).ExecuteAsync();

            // Assert

            Assert.AreEqual(6, result.Sum());
        }

        [Test(Description = "https://github.com/couchbaselabs/Linq2Couchbase/issues/191")]
        public async Task ExecuteAsync_EnumerableQueryWithAdditionalFunction_ReturnsResult()
        {
            // Arrange

            var query = Enumerable.Range(1, 100).AsQueryable();

            // Act

            var result = await query.ExecuteAsync(p => p.Sum());

            // Assert

            Assert.AreEqual(query.Sum(), result);
        }

        #endregion
    }
}
