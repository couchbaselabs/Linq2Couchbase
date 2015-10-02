using System;
using System.Linq;
using System.Linq.Expressions;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Linq.QueryGeneration;
using Moq;
using Newtonsoft.Json.Serialization;
using Remotion.Linq;

namespace Couchbase.Linq.Tests
{
// ReSharper disable once InconsistentNaming
    public class N1QLTestBase
    {
        private IContractResolver _contractResolver = new DefaultContractResolver();
        public IContractResolver ContractResolver
        {
            get { return _contractResolver; }
        }

        protected virtual bool IsClusterRequired
        {
            get { return false; }
        }

        private BucketQueryExecutorEmulator _queryExecutor;
        protected virtual BucketQueryExecutorEmulator QueryExecutor
        {
            get
            {
                if (_queryExecutor == null)
                {
                    _queryExecutor = new BucketQueryExecutorEmulator(this);
                }

                return _queryExecutor;
            }
        }

        public N1QLTestBase()
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            if (IsClusterRequired)
            {
                InitializeCluster();
            }
        }

        protected string CreateN1QlQuery(IBucket bucket, Expression expression)
        {
            var queryModel = QueryParserHelper.CreateQueryParser().GetParsedQuery(expression);

            var queryGenerationContext = new N1QlQueryGenerationContext()
            {
                MemberNameResolver = new JsonNetMemberNameResolver(_contractResolver),
                MethodCallTranslatorProvider = new DefaultMethodCallTranslatorProvider(),
                Serializer = new Core.Serialization.DefaultSerializer()
            };

            var visitor = new N1QlQueryModelVisitor(queryGenerationContext);
            visitor.VisitQueryModel(queryModel);
            return visitor.GetQuery();
        }

        protected virtual IQueryable<T> CreateQueryable<T>(string bucketName)
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns(bucketName);

            return new BucketQueryable<T>(mockBucket.Object, 
                QueryParserHelper.CreateQueryParser(), QueryExecutor);
        }

        protected void InitializeCluster(IContractResolver contractResolver = null)
        {
            if (contractResolver != null)
            {
                _contractResolver = contractResolver;
            }
            var config = TestConfigurations.DefaultConfig();
            //var config = new ClientConfiguration();
            //config.Servers.Add(new Uri("http://127.0.0.1:8091"));
            config.DeserializationSettings.ContractResolver = _contractResolver;
            config.SerializationSettings.ContractResolver = _contractResolver;
            ClusterHelper.Initialize(config);
        }

        protected void SetContractResolver(IContractResolver contractResolver)
        {
            _contractResolver = contractResolver;

            if (IsClusterRequired)
            {
                var cluster = ClusterHelper.Get();
                cluster.Configuration.DeserializationSettings.ContractResolver = contractResolver;
                cluster.Configuration.SerializationSettings.ContractResolver = contractResolver;
            }
        }
    }
}