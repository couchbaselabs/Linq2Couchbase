using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Core.Version;
using Couchbase.Linq.Execution;
using Couchbase.Linq.QueryGeneration;
using Remotion.Linq;

namespace Couchbase.Linq.UnitTests
{
    /// <summary>
    /// Used to test query generation for result operators that always execute the query immediately.
    /// This class fakes the result (always returns null or an empty list), but stores the query that was
    /// generated in the Query property.
    /// </summary>
    internal class ClusterQueryExecutorEmulator : IAsyncQueryExecutor
    {
        private readonly ClusterVersion _clusterVersion;

        public N1QLTestBase Test { get; }

        public string Query { get; private set; }

        public ClusterQueryExecutorEmulator(N1QLTestBase test, ClusterVersion clusterVersion)
        {
            Test = test ?? throw new ArgumentNullException(nameof(test));
            _clusterVersion = clusterVersion;
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            Query = ExecuteCollection(queryModel);

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
                LoggerFactory = Test.LoggerFactory
            };

            var visitor = new N1QlQueryModelVisitor(queryGenerationContext);
            visitor.VisitQueryModel(queryModel);
            return visitor.GetQuery();
        }

        public IAsyncEnumerable<T> ExecuteCollectionAsync<T>(QueryModel queryModel, CancellationToken cancellationToken = default)
        {
            ExecuteCollection<T>(queryModel);

            return AsyncEnumerable.Empty<T>();
        }

        public Task<T> ExecuteSingleAsync<T>(QueryModel queryModel, bool returnDefaultWhenEmpty,
            CancellationToken cancellationToken = default)
        {
            ExecuteCollection<T>(queryModel);

            return Task.FromResult(default(T));
        }
    }
}