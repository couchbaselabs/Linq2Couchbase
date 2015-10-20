using System;
using System.Linq;
using System.Linq.Expressions;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Linq.QueryGeneration;
using Moq;
using Newtonsoft.Json.Serialization;
using Remotion.Linq;

namespace Couchbase.Linq.IntegrationTests
{
// ReSharper disable once InconsistentNaming
    public class N1QLTestBase
    {
        private IContractResolver _contractResolver = new DefaultContractResolver();
        public IContractResolver ContractResolver
        {
            get { return _contractResolver; }
        }

        public N1QLTestBase()
        {
            InitializeCluster();
        }

        protected void InitializeCluster(IContractResolver contractResolver = null)
        {
            if (contractResolver != null)
            {
                _contractResolver = contractResolver;
            }

            var config = TestConfigurations.DefaultConfig();
            config.DeserializationSettings.ContractResolver = _contractResolver;
            config.SerializationSettings.ContractResolver = _contractResolver;
            ClusterHelper.Initialize(config);
        }

        protected void SetContractResolver(IContractResolver contractResolver)
        {
            _contractResolver = contractResolver;

            var cluster = ClusterHelper.Get();
            cluster.Configuration.DeserializationSettings.ContractResolver = contractResolver;
            cluster.Configuration.SerializationSettings.ContractResolver = contractResolver;
        }
    }
}