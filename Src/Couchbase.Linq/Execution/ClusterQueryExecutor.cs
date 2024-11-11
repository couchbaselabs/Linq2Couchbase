using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Core.IO.Serializers;
using Couchbase.Core.Version;
using Couchbase.Linq.Clauses;
using Couchbase.Linq.QueryGeneration;
using Couchbase.Linq.QueryGeneration.MemberNameResolvers;
using Couchbase.Linq.Utils;
using Couchbase.Linq.Versioning;
using Couchbase.Query;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Remotion.Linq;

namespace Couchbase.Linq.Execution
{
    internal class ClusterQueryExecutor : IAsyncQueryExecutor
    {
        private readonly ICluster _cluster;
        private readonly ILogger<ClusterQueryExecutor> _logger;
        private readonly IClusterVersionProvider _clusterVersionProvider;

        private ITypeSerializer? _serializer;

        private ITypeSerializer Serializer =>
            _serializer ??= _cluster.ClusterServices.GetRequiredService<ITypeSerializer>();

        /// <summary>
        /// Query timeout callback, if null uses the cluster default.
        /// </summary>
        public Func<TimeSpan?>? QueryTimeoutProvider { get; set; }

        /// <summary>
        /// Creates a new BucketQueryExecutor.
        /// </summary>
        /// <param name="cluster"><see cref="ICluster"/> to query.</param>
        public ClusterQueryExecutor(ICluster cluster)
        {
            _cluster = cluster;
            _logger = cluster.ClusterServices.GetRequiredService<ILogger<ClusterQueryExecutor>>();
            _clusterVersionProvider = cluster.ClusterServices.GetRequiredService<IClusterVersionProvider>();
        }

        private LinqQueryOptions GetQueryOptions(QueryModel queryModel, ScalarResultBehavior scalarResultBehavior)
        {
            var queryOptions = new LinqQueryOptions(scalarResultBehavior);

            MutationState? combinedMutationState = null;

            foreach (var bodyClause in queryModel.BodyClauses)
            {
                switch (bodyClause)
                {
                    case ScanConsistencyClause scanConsistency:
                        queryOptions.ScanConsistency(scanConsistency.ScanConsistency);

                        if (scanConsistency.ScanWait != null)
                        {
                            queryOptions.ScanWait(scanConsistency.ScanWait.Value);
                        }
                        break;

                    case ConsistentWithClause consistentWith:
                        combinedMutationState ??= new MutationState();
                        combinedMutationState.Add(consistentWith.MutationState);

                        if (consistentWith.ScanWait != null)
                        {
                            queryOptions.ScanWait(consistentWith.ScanWait.Value);
                        }
                        break;
                }
            }

            if (combinedMutationState != null)
            {
                queryOptions.ConsistentWith(combinedMutationState);
            }

            var queryTimeout = QueryTimeoutProvider?.Invoke();
            if (queryTimeout is not null)
            {
                queryOptions.Timeout(queryTimeout.GetValueOrDefault());
            }

            return queryOptions;
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var statement = GenerateQuery(queryModel, out var scalarResultBehavior);

            return ExecuteCollection<T>(statement, GetQueryOptions(queryModel, scalarResultBehavior));
        }

        /// <summary>
        /// Execute a <see cref="LinqQueryOptions"/>.
        /// </summary>
        /// <typeparam name="T">Type returned by the query.</typeparam>
        /// <param name="statement">Query to execute.</param>
        /// <param name="queryOptions">Options to control execution.</param>
        /// <returns>List of objects returned by the request.</returns>
        public IEnumerable<T> ExecuteCollection<T>(string statement, LinqQueryOptions queryOptions)
        {
            return ExecuteCollectionAsync<T>(statement, queryOptions).ToEnumerable();
        }

        public IAsyncEnumerable<T> ExecuteCollectionAsync<T>(QueryModel queryModel, CancellationToken cancellationToken = default)
        {
            var statement = GenerateQuery(queryModel, out var scalarResultBehavior);

            return ExecuteCollectionAsync<T>(statement, GetQueryOptions(queryModel, scalarResultBehavior), cancellationToken);
        }

        /// <summary>
        /// Asynchronously execute a <see cref="LinqQueryOptions"/>.
        /// </summary>
        /// <typeparam name="T">Type returned by the query.</typeparam>
        /// <param name="statement">Query to execute.</param>
        /// <param name="queryOptions">Options to control execution.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task which contains a list of objects returned by the request when complete.</returns>
        public async IAsyncEnumerable<T> ExecuteCollectionAsync<T>(string statement, LinqQueryOptions queryOptions,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // TODO: Make this more efficient with a custom enumerator

            queryOptions.CancellationToken(cancellationToken);

            IAsyncEnumerable<T> result;

            if (!queryOptions.ScalarResultBehavior.ResultExtractionRequired)
            {
                result = await _cluster.QueryAsync<T>(statement, queryOptions).ConfigureAwait(false);
            }
            else
            {
                var tempResult = await _cluster.QueryAsync<ScalarResult<T>>(statement, queryOptions).ConfigureAwait(false);

                result = queryOptions.ScalarResultBehavior.ApplyResultExtraction(tempResult);
            }

            await foreach (var row in result.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                yield return row;
            }
        }

        public T ExecuteScalar<T>(QueryModel queryModel)=>
            ExecuteSingle<T>(queryModel, false)!;

        public T? ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty) =>
            returnDefaultWhenEmpty
                ? ExecuteCollection<T>(queryModel).SingleOrDefault()
                : ExecuteCollection<T>(queryModel).Single();

        public Task<T> ExecuteScalarAsync<T>(QueryModel queryModel, CancellationToken cancellationToken = default) =>
            ExecuteSingleAsync<T>(queryModel, false, cancellationToken);

        public Task<T> ExecuteSingleAsync<T>(QueryModel queryModel, bool returnDefaultWhenEmpty, CancellationToken cancellationToken = default)
        {
            // ReSharper disable MethodSupportsCancellation
            var result = returnDefaultWhenEmpty
                ? ExecuteCollectionAsync<T>(queryModel).SingleOrDefaultAsync(cancellationToken)
                : ExecuteCollectionAsync<T>(queryModel).SingleAsync(cancellationToken);
            // ReSharper restore MethodSupportsCancellation

            return result.AsTask();
        }

        public string GenerateQuery(QueryModel queryModel, out ScalarResultBehavior scalarResultBehavior)
        {
            // If ITypeSerializer is an IExtendedTypeSerializer, use it as the member name resolver
            // Otherwise fallback to the legacy behavior which assumes we're using Newtonsoft.Json
            // Note that DefaultSerializer implements IExtendedTypeSerializer, but has the same logic as JsonNetMemberNameResolver

            var serializer = Serializer as IExtendedTypeSerializer;

            var memberNameResolver = serializer != null ?
                (IMemberNameResolver)new ExtendedTypeSerializerMemberNameResolver(serializer) :
                (IMemberNameResolver)new JsonNetMemberNameResolver(JsonConvert.DefaultSettings!().ContractResolver!);

            var methodCallTranslatorProvider = new DefaultMethodCallTranslatorProvider();

            var clusterVersionTask = _clusterVersionProvider.GetVersionAsync();
            var clusterVersion = clusterVersionTask.IsCompleted
                ? clusterVersionTask.Result
                // TODO: Don't use .Result to block
                : clusterVersionTask.AsTask().Result; // Must convert ValueTask to Task to safely await the result if it is not completed

            var queryGenerationContext = new N1QlQueryGenerationContext
            {
                MemberNameResolver = memberNameResolver,
                MethodCallTranslatorProvider = methodCallTranslatorProvider,
                Serializer = serializer,
                ClusterVersion = clusterVersion ?? FeatureVersions.DefaultVersion,
                LoggerFactory = _cluster.ClusterServices.GetRequiredService<ILoggerFactory>()
            };

            var visitor = new N1QlQueryModelVisitor(queryGenerationContext);
            visitor.VisitQueryModel(queryModel);

            var query = visitor.GetQuery();
            _logger.LogDebug("Generated query: {query}", query);

            scalarResultBehavior = visitor.ScalarResultBehavior;
            return query;
        }
    }
}