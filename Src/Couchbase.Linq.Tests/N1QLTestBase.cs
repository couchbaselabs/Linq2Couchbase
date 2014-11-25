using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Linq.QueryGeneration;
using Remotion.Linq.Parsing.Structure;
using Newtonsoft.Json.Serialization;

namespace Couchbase.Linq.Tests
{
// ReSharper disable once InconsistentNaming
    public class N1QLTestBase
    {
        public N1QLTestBase()
        {
            InitializeCluster();
        }

        protected string CreateN1QlQuery(IBucket bucket, Expression expression)
        {
            var queryModel = QueryParserHelper.CreateQueryParser().GetParsedQuery(expression);
            return N1QlQueryModelVisitor.GenerateN1QlQuery(queryModel, bucket.Name);
        }

        protected void InitializeCluster(IContractResolver contractResolver = null)
        {
            if (contractResolver == null)
            {
                contractResolver = new DefaultContractResolver();
            }

            var config = new Couchbase.Configuration.Client.ClientConfiguration();
            config.Servers.Add(new Uri("http://127.0.0.1:8091"));
            config.DeserializationSettings.ContractResolver = contractResolver;
            config.SerializationSettings.ContractResolver = contractResolver;
            ClusterHelper.Initialize(config);
        }

        protected void SetContractResolver(IContractResolver contractResolver)
        {
            var cluster = ClusterHelper.Get();
            cluster.Configuration.DeserializationSettings.ContractResolver = contractResolver;
            cluster.Configuration.SerializationSettings.ContractResolver = contractResolver;
        }

    }
}
