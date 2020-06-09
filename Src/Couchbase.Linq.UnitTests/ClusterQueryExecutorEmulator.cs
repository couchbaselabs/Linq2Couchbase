using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Core.Version;
using Couchbase.Linq.Execution;
using Couchbase.Linq.QueryGeneration;
using Couchbase.Query;
using Microsoft.Extensions.Logging;
using Remotion.Linq;

namespace Couchbase.Linq.UnitTests
{
    /// <summary>
    /// Used to test query generation for result operators that always execute the query immediately.
    /// This class fakes the result (always returns null or an empty list), but stores the query that was
    /// generated in the Query property.
    /// </summary>
    internal class ClusterQueryExecutorEmulator : IClusterQueryExecutor
    {
        public QueryScanConsistency? ScanConsistency { get; set; }
        public TimeSpan? ScanWait { get; set; }
        public TimeSpan? Timeout { get; set; }

        private readonly N1QLTestBase _test;
        private readonly ClusterVersion _clusterVersion;

        public N1QLTestBase Test
        {
            get { return _test; }
        }

        private string _query;
        public string Query
        {
            get { return _query; }
        }

        public bool UseStreaming { get; set; }

        public ClusterQueryExecutorEmulator(N1QLTestBase test, ClusterVersion clusterVersion)
        {
            _test = test ?? throw new ArgumentNullException(nameof(test));
            _clusterVersion = clusterVersion;
        }

        public void ConsistentWith(MutationState state)
        {
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            _query = ExecuteCollection(queryModel);

            return new T[] {};
        }

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            ExecuteCollection<T>(queryModel);
            return default(T);
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            ExecuteCollection<T>(queryModel);
            return default(T);
        }

        public string ExecuteCollection(QueryModel queryModel)
        {
            var queryGenerationContext = new N1QlQueryGenerationContext()
            {
                MemberNameResolver = Test.MemberNameResolver,
                MethodCallTranslatorProvider = new DefaultMethodCallTranslatorProvider(),
                Serializer = new Core.IO.Serializers.DefaultSerializer(),
                ClusterVersion = _clusterVersion,
                LoggerFactory = _test.LoggerFactory
            };

            var visitor = new N1QlQueryModelVisitor(queryGenerationContext);
            visitor.VisitQueryModel(queryModel);
            return visitor.GetQuery();
        }

        public IAsyncEnumerable<T> ExecuteCollectionAsync<T>(QueryModel queryModel, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}