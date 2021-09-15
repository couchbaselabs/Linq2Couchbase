using System.Linq;
using Couchbase.Core.IO.Serializers;
using Couchbase.Core.Version;
using Couchbase.KeyValue;
using Couchbase.Linq.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Couchbase.Linq.UnitTests
{
    internal class QueryFactory
    {
        public static IQueryable<T> Queryable<T>(IBucket bucket) =>
            Queryable<T>(bucket.Name, "_default", "_default");

        public static IQueryable<T> Queryable<T>(IBucket bucket, string scopeName, string collectionName) =>
            Queryable<T>(bucket.Name, scopeName, collectionName);

        public static IQueryable<T> Queryable<T>(string bucketName) =>
            Queryable<T>(bucketName, "_default", "_default");

        public static IQueryable<T> Queryable<T>(string bucketName, string scopeName, string collectionName)
        {
            var serializer = new DefaultSerializer();

            var services = new ServiceCollection();

            services.AddSingleton<ITypeSerializer>(serializer);
            services.AddLogging();
            services.AddSingleton(Mock.Of<IClusterVersionProvider>());
            services.AddSingleton<ISerializationConverterProvider>(
                new DefaultSerializationConverterProvider(serializer,
                    TypeBasedSerializationConverterRegistry.CreateDefaultRegistry()));

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
            mockCollection
                .SetupGet(p => p.Scope.Name)
                .Returns(scopeName);
            mockCollection
                .SetupGet(p => p.Name)
                .Returns(collectionName);

            return new CollectionQueryable<T>(mockCollection.Object, default);
        }
    }
}