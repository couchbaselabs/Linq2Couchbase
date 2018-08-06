using System;
using System.Linq;
using System.Linq.Expressions;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Core.Version;
using Couchbase.Linq.Execution;
using Couchbase.Linq.QueryGeneration;
using Couchbase.Linq.QueryGeneration.MemberNameResolvers;
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

        private BucketQueryExecutorEmulator _queryExecutor;
        internal BucketQueryExecutorEmulator QueryExecutor
        {
            get
            {
                if (_queryExecutor == null)
                {
                    _queryExecutor = new BucketQueryExecutorEmulator(this, DefaultClusterVersion);
                }

                return _queryExecutor;
            }
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
            var serializer = new Core.Serialization.DefaultSerializer();

            var bucketContext = new Mock<IBucketContext>();
            bucketContext.SetupGet(p => p.Bucket).Returns(bucket);
            bucketContext.SetupGet(p => p.Configuration).Returns(new ClientConfiguration
            {
                Serializer = () => serializer
            });

            var queryModel = QueryParserHelper.CreateQueryParser(bucketContext.Object).GetParsedQuery(expression);

            var queryGenerationContext = new N1QlQueryGenerationContext()
            {
                MemberNameResolver = MemberNameResolver,
                MethodCallTranslatorProvider = new DefaultMethodCallTranslatorProvider(),
                Serializer = serializer,
                SelectDocumentMetadata = selectDocumentMetadata,
                ClusterVersion = clusterVersion
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

        internal virtual IQueryable<T> CreateQueryable<T>(string bucketName, IBucketQueryExecutor queryExecutor)
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns(bucketName);

            var serializer = new Core.Serialization.DefaultSerializer();

            var bucketContext = new Mock<IBucketContext>();
            bucketContext.SetupGet(p => p.Bucket).Returns(mockBucket.Object);
            bucketContext.SetupGet(p => p.Configuration).Returns(new ClientConfiguration
            {
                Serializer = () => serializer
            });

            return new BucketQueryable<T>(mockBucket.Object,
                QueryParserHelper.CreateQueryParser(bucketContext.Object), queryExecutor);
        }

        protected void SetContractResolver(IContractResolver contractResolver)
        {
            _memberNameResolver = new JsonNetMemberNameResolver(contractResolver);
        }
    }
}