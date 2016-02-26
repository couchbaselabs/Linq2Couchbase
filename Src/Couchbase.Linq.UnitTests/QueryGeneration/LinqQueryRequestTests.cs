using System.Linq;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Linq.QueryGeneration;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    public class LinqQueryRequestTests
    {
        [Test]
        public void CreateQueryRequest_NoParameters_ReturnsQueryWrappedInQueryRequest()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.Name).Returns("default");
            bucket.Setup(m => m.Configuration).Returns(new BucketConfiguration()
            {
                PoolConfiguration = new PoolConfiguration(new ClientConfiguration())
            });

            var context = new BucketContext(bucket.Object);
            var query = context.Query<Brewery>().Where(p => p.Name == "name");

            // Act

            var result = LinqQueryRequest.CreateQueryRequest(query);

            // Assert

            const string queryStr = "SELECT `Extent1`.* FROM `default` as `Extent1` WHERE (`Extent1`.`name` = 'name')";

            Assert.NotNull(result);
            Assert.AreEqual(queryStr, result.GetOriginalStatement());
        }

        [Test]
        public void CreateQueryRequest_WithAggregate_ReturnsQueryWrappedInQueryRequest()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.Name).Returns("default");
            bucket.Setup(m => m.Configuration).Returns(new BucketConfiguration()
            {
                PoolConfiguration = new PoolConfiguration(new ClientConfiguration())
            });

            var context = new BucketContext(bucket.Object);
            var query = context.Query<Beer>().Where(p => p.Name == "name");

            // Act

            var result = LinqQueryRequest.CreateQueryRequest(query, p => p.Average(q => q.Abv));

            // Assert

            const string queryStr = "SELECT AVG(`Extent1`.`abv`) as `result` FROM `default` as `Extent1` WHERE (`Extent1`.`name` = 'name')";

            Assert.NotNull(result);
            Assert.AreEqual(queryStr, result.GetOriginalStatement());
            Assert.True(result.ScalarResultBehavior.ResultExtractionRequired);
        }

        [Test]
        public void CreateQueryRequest_First_ReturnsQueryWrappedInQueryRequest()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.Name).Returns("default");
            bucket.Setup(m => m.Configuration).Returns(new BucketConfiguration()
            {
                PoolConfiguration = new PoolConfiguration(new ClientConfiguration())
            });

            var context = new BucketContext(bucket.Object);
            var query = context.Query<Brewery>().Where(p => p.Name == "name");

            // Act

            var result = LinqQueryRequest.CreateQueryRequest(query, p => p.First());

            // Assert

            const string queryStr = "SELECT `Extent1`.* FROM `default` as `Extent1` WHERE (`Extent1`.`name` = 'name') LIMIT 1";

            Assert.NotNull(result);
            Assert.AreEqual(queryStr, result.GetOriginalStatement());
            Assert.False(result.ReturnDefaultWhenEmpty);
        }

        [Test]
        public void CreateQueryRequest_FirstOrDefault_ReturnsQueryWrappedInQueryRequest()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.Name).Returns("default");
            bucket.Setup(m => m.Configuration).Returns(new BucketConfiguration()
            {
                PoolConfiguration = new PoolConfiguration(new ClientConfiguration())
            });

            var context = new BucketContext(bucket.Object);
            var query = context.Query<Brewery>().Where(p => p.Name == "name");

            // Act

            var result = LinqQueryRequest.CreateQueryRequest(query, p => p.FirstOrDefault());

            // Assert

            const string queryStr = "SELECT `Extent1`.* FROM `default` as `Extent1` WHERE (`Extent1`.`name` = 'name') LIMIT 1";

            Assert.NotNull(result);
            Assert.AreEqual(queryStr, result.GetOriginalStatement());
            Assert.True(result.ReturnDefaultWhenEmpty);
        }

        [Test]
        public void CreateQueryRequest_Any_ReturnsQueryWrappedInQueryRequest()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.Name).Returns("default");
            bucket.Setup(m => m.Configuration).Returns(new BucketConfiguration()
            {
                PoolConfiguration = new PoolConfiguration(new ClientConfiguration())
            });

            var context = new BucketContext(bucket.Object);
            var query = context.Query<Brewery>();

            // Act

            var result = LinqQueryRequest.CreateQueryRequest(query, p => p.Any(q => q.Name == "name"));

            // Assert

            const string queryStr = "SELECT true as result FROM `default` as `Extent1` WHERE (`Extent1`.`name` = 'name') LIMIT 1";

            Assert.NotNull(result);
            Assert.AreEqual(queryStr, result.GetOriginalStatement());
            Assert.True(result.ScalarResultBehavior.ResultExtractionRequired);
            Assert.AreEqual(false, result.ScalarResultBehavior.NoRowsResult);
        }

        [Test]
        public void CreateQueryRequest_All_ReturnsQueryWrappedInQueryRequest()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.Name).Returns("default");
            bucket.Setup(m => m.Configuration).Returns(new BucketConfiguration()
            {
                PoolConfiguration = new PoolConfiguration(new ClientConfiguration())
            });

            var context = new BucketContext(bucket.Object);
            var query = context.Query<Brewery>();

            // Act

            var result = LinqQueryRequest.CreateQueryRequest(query, p => p.All(q => q.Name == "name"));

            // Assert

            const string queryStr = "SELECT false as result FROM `default` as `Extent1` WHERE NOT ((`Extent1`.`name` = 'name')) LIMIT 1";

            Assert.NotNull(result);
            Assert.AreEqual(queryStr, result.GetOriginalStatement());
            Assert.True(result.ScalarResultBehavior.ResultExtractionRequired);
            Assert.AreEqual(true, result.ScalarResultBehavior.NoRowsResult);
        }
    }
}

#region [ License information          ]

/* ************************************************************
 *
 *    @author Couchbase <info@couchbase.com>
 *    @copyright 2015 Couchbase, Inc.
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *
 * ************************************************************/

#endregion
