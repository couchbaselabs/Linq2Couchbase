using System.Linq;
using Couchbase.Core.IO.Serializers;
using Couchbase.Core.Version;
using Couchbase.KeyValue;
using Couchbase.Linq.Execution;
using Couchbase.Linq.Filters;
using Couchbase.Linq.QueryGeneration;
using Couchbase.Linq.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Couchbase.Linq.UnitTests
{
    internal class QueryFactory
    {
        public static IQueryable<T> Queryable<T>(IBucket bucket) =>
            Queryable<T>(bucket.Name, N1QlHelpers.DefaultScopeName, N1QlHelpers.DefaultCollectionName);

        public static IQueryable<T> Queryable<T>(IBucket bucket, string scopeName, string collectionName) =>
            Queryable<T>(bucket.Name, scopeName, collectionName);

        public static IQueryable<T> Queryable<T>(string bucketName) =>
            Queryable<T>(bucketName, N1QlHelpers.DefaultScopeName, N1QlHelpers.DefaultCollectionName);

        public static IQueryable<T> Queryable<T>(string bucketName, string scopeName, string collectionName) =>
            Queryable<T>(bucketName, scopeName, collectionName, Mock.Of<IAsyncQueryExecutor>());

        public static IQueryable<T> Queryable<T>(string bucketName, string scopeName, string collectionName, IAsyncQueryExecutor queryExecutor)
        {
            var mockCollection = CreateMockCollection(bucketName, scopeName, collectionName);

            return new CollectionQueryable<T>(mockCollection,
                new ClusterQueryProvider(
                    QueryParserHelper.CreateQueryParser(mockCollection.Scope.Bucket.Cluster),
                    queryExecutor));
        }

        public static ICouchbaseCollection CreateMockCollection(string bucketName, string scopeName, string collectionName) =>
            CreateMockBucket(bucketName).Scope(scopeName).Collection(collectionName);

        public static IBucket CreateMockBucket(string bucketName)
        {
            var serializer = new DefaultSerializer();

            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<ITypeSerializer>(serializer);
            services.AddSingleton(new DocumentFilterManager());
            services.Add(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(NullLogger<>)));
            services.AddSingleton(Mock.Of<IClusterVersionProvider>());
            services.AddSingleton<ISerializationConverterProvider>(
                new DefaultSerializationConverterProvider(serializer,
                    TypeBasedSerializationConverterRegistry.CreateDefaultRegistry()));

            var mockCluster = new Mock<ICluster>();
            mockCluster
                .Setup(p => p.ClusterServices)
                .Returns(services.BuildServiceProvider());

            var mockBucket = new Mock<IBucket>();
            mockBucket
                .SetupGet(e => e.Name)
                .Returns(bucketName);
            mockBucket
                .SetupGet(e => e.Cluster)
                .Returns(mockCluster.Object);
            mockBucket
                .Setup(e => e.Scope(It.IsAny<string>()))
                .Returns((string scopeName) =>
                {
                    var mockScope = new Mock<IScope>();
                    mockScope
                        .SetupGet(p => p.Name)
                        .Returns(scopeName);
                    mockScope
                        .SetupGet(p => p.Bucket)
                        .Returns(mockBucket.Object);
                    mockScope
                        .Setup(e => e.Collection(It.IsAny<string>()))
                        .Returns((string collectionName) =>
                        {
                            var mockCollection = new Mock<ICouchbaseCollection>();
                            mockCollection
                                .SetupGet(p => p.Name)
                                .Returns(collectionName);
                            mockCollection
                                .SetupGet(p => p.Scope)
                                .Returns(mockScope.Object);

                            return mockCollection.Object;
                        });

                    return mockScope.Object;
                });

            return mockBucket.Object;
        }
    }
}