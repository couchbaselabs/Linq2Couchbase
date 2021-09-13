using System.Linq.Expressions;
using Couchbase.KeyValue;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests
{
    [TestFixture]
    public class BucketContextTests : N1QLTestBase
    {
        [Test]
        public void Can_Get_The_Bucket_The_Context_Was_Created_With()
        {
            var mockCluster = new Mock<ICluster>();
            mockCluster
                .Setup(p => p.ClusterServices)
                .Returns(ServiceProvider);

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");
            mockBucket.SetupGet(e => e.Cluster).Returns(mockCluster.Object);

            var context = new BucketContext(mockBucket.Object);

            Assert.AreSame(mockBucket.Object, context.Bucket);
        }

        [Test]
        public void Query_NoOptions_AppliesFilters()
        {
            // Arrange

            var mockCluster = new Mock<ICluster>();
            mockCluster
                .Setup(p => p.ClusterServices)
                .Returns(ServiceProvider);

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");
            mockBucket.SetupGet(e => e.Cluster).Returns(mockCluster.Object);

            var mockScope = new Mock<IScope>();
            mockScope.SetupGet(e => e.Bucket).Returns(mockBucket.Object);

            var mockCollection = new Mock<ICouchbaseCollection>();
            mockCollection.SetupGet(e => e.Scope).Returns(() => mockScope.Object);

            mockBucket.Setup(e => e.Scope("_default")).Returns(mockScope.Object);
            mockBucket.Setup(e => e.DefaultScope()).Returns(mockScope.Object);
            mockBucket.Setup(e => e.DefaultCollection()).Returns(mockCollection.Object);
            mockScope.Setup(e => e.Collection("_default")).Returns(mockCollection.Object);

            var ctx = new BucketContext(mockBucket.Object);

            // Act

            var query = ctx.Query<BeerFiltered>();

            // Assert

            var methodCall = query.Expression as MethodCallExpression;
            Assert.IsNotNull(methodCall);
            Assert.AreEqual("Where", methodCall.Method.Name);
        }

        [Test]
        public void Query_None_AppliesFilters()
        {
            // Arrange

            var mockCluster = new Mock<ICluster>();
            mockCluster
                .Setup(p => p.ClusterServices)
                .Returns(ServiceProvider);

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");
            mockBucket.SetupGet(e => e.Cluster).Returns(mockCluster.Object);

            var mockScope = new Mock<IScope>();
            mockScope.SetupGet(e => e.Bucket).Returns(mockBucket.Object);

            var mockCollection = new Mock<ICouchbaseCollection>();
            mockCollection.SetupGet(e => e.Scope).Returns(() => mockScope.Object);

            mockBucket.Setup(e => e.Scope("_default")).Returns(mockScope.Object);
            mockBucket.Setup(e => e.DefaultScope()).Returns(mockScope.Object);
            mockBucket.Setup(e => e.DefaultCollection()).Returns(mockCollection.Object);
            mockScope.Setup(e => e.Collection("_default")).Returns(mockCollection.Object);

            var ctx = new BucketContext(mockBucket.Object);

            // Act

            var query = ctx.Query<BeerFiltered>(BucketQueryOptions.None);

            // Assert

            var methodCall = query.Expression as MethodCallExpression;
            Assert.IsNotNull(methodCall);
            Assert.AreEqual("Where", methodCall.Method.Name);
        }

        [Test]
        public void Query_SuppressFilters_DoesntApplyFilters()
        {
            // Arrange

            var mockCluster = new Mock<ICluster>();
            mockCluster
                .Setup(p => p.ClusterServices)
                .Returns(ServiceProvider);

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");
            mockBucket.SetupGet(e => e.Cluster).Returns(mockCluster.Object);

            var mockScope = new Mock<IScope>();
            mockScope.SetupGet(e => e.Bucket).Returns(mockBucket.Object);

            var mockCollection = new Mock<ICouchbaseCollection>();
            mockCollection.SetupGet(e => e.Scope).Returns(() => mockScope.Object);

            mockBucket.Setup(e => e.Scope("_default")).Returns(mockScope.Object);
            mockBucket.Setup(e => e.DefaultScope()).Returns(mockScope.Object);
            mockBucket.Setup(e => e.DefaultCollection()).Returns(mockCollection.Object);
            mockScope.Setup(e => e.Collection("_default")).Returns(mockCollection.Object);

            var ctx = new BucketContext(mockBucket.Object);

            // Act

            var query = ctx.Query<BeerFiltered>(BucketQueryOptions.SuppressFilters);

            // Assert

            Assert.IsAssignableFrom<ConstantExpression>(query.Expression);
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
