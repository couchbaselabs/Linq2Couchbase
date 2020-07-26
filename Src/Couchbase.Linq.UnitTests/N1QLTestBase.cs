using System;
using System.Linq;
using System.Linq.Expressions;
using Couchbase.Core.IO.Serializers;
using Couchbase.Core.Version;
using Couchbase.KeyValue;
using Couchbase.Linq.Execution;
using Couchbase.Linq.Filters;
using Couchbase.Linq.QueryGeneration;
using Couchbase.Linq.QueryGeneration.MemberNameResolvers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Serialization;

namespace Couchbase.Linq.UnitTests
{
// ReSharper disable once InconsistentNaming
    public class N1QLTestBase
    {
        protected static readonly ClusterVersion DefaultClusterVersion = new ClusterVersion(new Version(4, 0, 0));

        private IMemberNameResolver _memberNameResolver = new JsonNetMemberNameResolver(new DefaultContractResolver());
        internal IMemberNameResolver MemberNameResolver
        {
            get { return _memberNameResolver; }
        }

        private ClusterQueryExecutorEmulator _queryExecutor;
        internal ClusterQueryExecutorEmulator QueryExecutor
        {
            get
            {
                if (_queryExecutor == null)
                {
                    _queryExecutor = new ClusterQueryExecutorEmulator(this, DefaultClusterVersion);
                }

                return _queryExecutor;
            }
        }

        public IServiceProvider ServiceProvider { get; }
        public ILoggerFactory LoggerFactory => ServiceProvider.GetRequiredService<ILoggerFactory>();

        protected N1QLTestBase()
        {
            var serializer = new DefaultSerializer();

            var services = new ServiceCollection();

            services.AddSingleton(new DocumentFilterManager());
            services.AddSingleton<ITypeSerializer>(serializer);
            services.AddLogging();
            services.AddSingleton(Mock.Of<IClusterVersionProvider>());

            ServiceProvider = services.BuildServiceProvider();
        }

        protected string CreateN1QlQuery(IBucket bucket, Expression expression)
        {
            return CreateN1QlQuery(bucket, expression, false);
        }

        protected string CreateN1QlQuery(IBucket bucket, Expression expression, ClusterVersion clusterVersion)
        {
            return CreateN1QlQuery(bucket, expression, clusterVersion, false);
        }

        protected string CreateN1QlQuery(IBucket bucket, Expression expression, bool selectDocumentMetadata)
        {
            return CreateN1QlQuery(bucket, expression, DefaultClusterVersion, selectDocumentMetadata);
        }

        protected string CreateN1QlQuery(IBucket bucket, Expression expression, ClusterVersion clusterVersion,
            bool selectDocumentMetadata)
        {
            return CreateN1QlQuery(bucket, expression, clusterVersion, selectDocumentMetadata, out var _);
        }

        internal string CreateN1QlQuery(IBucket bucket, Expression expression, ClusterVersion clusterVersion,
            bool selectDocumentMetadata, out ScalarResultBehavior resultBehavior)
        {
            var mockCluster = new Mock<ICluster>();
            mockCluster
                .Setup(p => p.ClusterServices)
                .Returns(ServiceProvider);

            var queryModel = QueryParserHelper.CreateQueryParser(mockCluster.Object).GetParsedQuery(expression);

            var queryGenerationContext = new N1QlQueryGenerationContext()
            {
                MemberNameResolver = MemberNameResolver,
                MethodCallTranslatorProvider = new DefaultMethodCallTranslatorProvider(),
                Serializer = ServiceProvider.GetRequiredService<ITypeSerializer>(),
                SelectDocumentMetadata = selectDocumentMetadata,
                ClusterVersion = clusterVersion,
                LoggerFactory = LoggerFactory
            };

            var visitor = new N1QlQueryModelVisitor(queryGenerationContext);
            visitor.VisitQueryModel(queryModel);

            resultBehavior = visitor.ScalarResultBehavior;
            return visitor.GetQuery();
        }

        protected virtual IQueryable<T> CreateQueryable<T>(string bucketName)
        {
            return CreateQueryable<T>(bucketName, QueryExecutor);
        }

        internal virtual IQueryable<T> CreateQueryable<T>(string bucketName, IAsyncQueryExecutor queryExecutor)
        {
            var mockCluster = new Mock<ICluster>();
            mockCluster
                .Setup(p => p.ClusterServices)
                .Returns(ServiceProvider);

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns(bucketName);
            mockBucket.SetupGet(e => e.Cluster).Returns(mockCluster.Object);

            var mockCollection = new Mock<ICouchbaseCollection>();
            mockCollection
                .SetupGet(p => p.Scope.Bucket)
                .Returns(mockBucket.Object);

            return new CollectionQueryable<T>(mockCollection.Object,
                QueryParserHelper.CreateQueryParser(mockCluster.Object), queryExecutor);
        }

        protected void SetContractResolver(IContractResolver contractResolver)
        {
            _memberNameResolver = new JsonNetMemberNameResolver(contractResolver);
        }
    }
}