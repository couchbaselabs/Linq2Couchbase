using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.Extensions
{
    [TestFixture]
    public class QueryExtensionTests
    {
        #region AsAsyncEnumerable

        [Test]
        public void AsAsyncEnumerable_Implements_Returns()
        {
            // Arrange

            var mock = new Mock<IQueryable<Beer>>();
            mock.As<IAsyncEnumerable<Beer>>();

            // Act

            var result = mock.Object.AsAsyncEnumerable();

            // Assert

            Assert.NotNull(result);
        }

        [Test]
        public void AsAsyncEnumerable_DoesNotImplement_InvalidOperationException()
        {
            // Arrange

            var mock = new Mock<IQueryable<Beer>>();

            // Act/Assert

            Assert.Throws<InvalidOperationException>(() => mock.Object.AsAsyncEnumerable());
        }

        #endregion

        #region AsAsyncEnumerable

        private async IAsyncEnumerable<Beer> TestEnumerable([EnumeratorCancellation] CancellationToken ct = default)
        {
            await Task.Yield();
            yield return new Beer {Name = "A"};
            yield return new Beer {Name = "B"};
        }

        [Test]
        public async Task ToListAsync_Implements_Returns()
        {
            // Arrange

            var mock = new Mock<IQueryable<Beer>>();
            var asyncMock = mock.As<IAsyncEnumerable<Beer>>();
            asyncMock
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns((CancellationToken ct) => TestEnumerable().GetAsyncEnumerator(ct));

            // Act

            var result = await mock.Object.ToListAsync();

            // Assert

            Assert.IsNotEmpty(result);
        }

        [Test]
        public void ToListAsync_DoesNotImplement_InvalidOperationException()
        {
            // Arrange

            var mock = new Mock<IQueryable<Beer>>();

            // Act/Assert

            Assert.Throws<InvalidOperationException>(() => mock.Object.ToListAsync());
        }

        #endregion
    }
}
