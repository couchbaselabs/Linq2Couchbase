using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Core.Version;
using Couchbase.Linq.Execution;
using Couchbase.Linq.QueryGeneration;
using Couchbase.N1QL;
using Remotion.Linq;

namespace Couchbase.Linq.UnitTests
{
    /// <summary>
    /// Used to test query generation for result operators that always execute the query immediately.
    /// This class fakes the result (always returns null or an empty list), but stores the query that was
    /// generated in the Query property.
    /// </summary>
    internal class BucketQueryExecutorEmulator : IBucketQueryExecutor
    {
        public ScanConsistency? ScanConsistency { get; set; }
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

        public BucketQueryExecutorEmulator(N1QLTestBase test, ClusterVersion clusterVersion)
        {
            if (test == null)
            {
                throw new ArgumentNullException("test");
            }

            _test = test;
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
                Serializer = new Core.Serialization.DefaultSerializer(),
                ClusterVersion = _clusterVersion
            };

            var visitor = new N1QlQueryModelVisitor(queryGenerationContext);
            visitor.VisitQueryModel(queryModel);
            return visitor.GetQuery();
        }

        public Task<IEnumerable<T>> ExecuteCollectionAsync<T>(LinqQueryRequest queryResult, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<T> ExecuteSingleAsync<T>(LinqQueryRequest queryRequest, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}