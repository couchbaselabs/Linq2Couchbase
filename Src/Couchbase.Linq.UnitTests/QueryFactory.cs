using System;
using System.Linq;
using Couchbase.Core.IO.Serializers;
using Couchbase.Core.Version;
using Couchbase.KeyValue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Couchbase.Linq.UnitTests
{
    internal class QueryFactory
    {
        public static IQueryable<T> Queryable<T>(IBucket bucket) => Queryable<T>(bucket.Name);

        public static IQueryable<T> Queryable<T>(string bucketName)
        {
            var serializer = new DefaultSerializer();

            var services = new ServiceCollection();

            services.AddSingleton<ITypeSerializer>(serializer);
            services.AddLogging();
            services.AddSingleton(Mock.Of<IClusterVersionProvider>());

            var mockCluster = new Mock<ICluster>();
            mockCluster
                .Setup(p => p.ClusterServices)
                .Returns(services.BuildServiceProvider());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns(bucketName);
            mockBucket.SetupGet(e => e.Cluster).Returns(mockCluster.Object);

            var mockCollection = new Mock<ICouchbaseCollection>();
            mockCollection
                .SetupGet(p => p.Scope.Bucket)
                .Returns(mockBucket.Object);

            return new CollectionQueryable<T>(mockCollection.Object);
        }
    }
}