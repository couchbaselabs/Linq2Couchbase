using System;
using System.Linq.Expressions;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Linq.QueryGeneration;
using Newtonsoft.Json.Serialization;

namespace Couchbase.Linq.Tests
{
// ReSharper disable once InconsistentNaming
    public class N1QLTestBase
    {
        private IContractResolver _contractResolver = new DefaultContractResolver();

        protected virtual bool IsClusterRequired
        {
            get { return false; }
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
                MethodCallTranslatorProvider = new DefaultMethodCallTranslatorProvider()
            };

            var visitor = new N1QlQueryModelVisitor(queryGenerationContext);
            visitor.VisitQueryModel(queryModel);
            return visitor.GetQuery();
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