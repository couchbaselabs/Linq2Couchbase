using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Core.Buckets;
using Couchbase.IO;
using Couchbase.Linq.Metadata;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests
{
    [TestFixture]
    public class BucketContextTests
    {
        [Test]
        public void Can_Get_The_Bucket_The_Context_Was_Created_With()
        {
            var bucket = new Mock<IBucket>();
            var context = new BucketContext(bucket.Object);

            Assert.AreSame(bucket.Object, context.Bucket);
        }

        [Test]
        public void Query_NoOptions_AppliesFilters()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket.Setup(x => x.Configuration).Returns(new ClientConfiguration().BucketConfigs.First().Value);
            var ctx = new BucketContext(bucket.Object);

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

            var bucket = new Mock<IBucket>();
            bucket.Setup(x => x.Configuration).Returns(new ClientConfiguration().BucketConfigs.First().Value);
            var ctx = new BucketContext(bucket.Object);

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

            var bucket = new Mock<IBucket>();
            bucket.Setup(x => x.Configuration).Returns(new ClientConfiguration().BucketConfigs.First().Value);
            var ctx = new BucketContext(bucket.Object);

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
