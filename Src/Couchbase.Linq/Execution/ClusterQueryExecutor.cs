using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Core.IO.Serializers;
using Couchbase.Core.Version;
using Couchbase.Linq.Operators;
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
    internal class ClusterQueryExecutor : IClusterQueryExecutor
    {
        private readonly ICluster _cluster;
        private readonly ILogger<ClusterQueryExecutor> _logger;
        private readonly IClusterVersionProvider _clusterVersionProvider;

        private ITypeSerializer _serializer;

        /// <summary>
        /// Specifies the consistency guarantee/constraint for index scanning.
        /// </summary>
        public QueryScanConsistency? ScanConsistency { get; set; }

        /// <summary>
        /// Specifies the maximum time the client is willing to wait for an index to catch up to the consistency requirement in the request.
        /// If an index has to catch up, and the time is exceed doing so, an error is returned.
        /// </summary>
        public TimeSpan? ScanWait { get; set; }

        /// <summary>
        /// Specifies the maximum time the server should wait for the QueryRequest to execute.
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        public MutationState MutationState { get; private set; }

        private ITypeSerializer Serializer =>
            _serializer ??= _cluster.ClusterServices.GetRequiredService<ITypeSerializer>();

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

        /// <summary>
        /// Requires that the indexes but up to date with a <see cref="Query.MutationState"/> before the query is executed.
        /// </summary>
        /// <param name="state"><see cref="Query.MutationState"/> used for conistency controls.</param>
        /// <remarks>If called multiple times, the states from the calls are combined.</remarks>
        public void ConsistentWith(MutationState state)
        {
            if (state == null)
            {
                return;
            }

            if (MutationState == null)
            {
                MutationState = new MutationState();
            }

            MutationState.Add(state);
        }

        private void ApplyQueryOptionsSettings(LinqQueryOptions queryOptions)
        {
            if (ScanConsistency.HasValue)
            {
                queryOptions.ScanConsistency(ScanConsistency.Value);
            }
            if (ScanWait.HasValue)
            {
                queryOptions.ScanWait(ScanWait.Value);
            }
            if (Timeout.HasValue)
            {
                queryOptions.Timeout(Timeout.Value);
            }
            if (MutationState != null)
            {
                queryOptions.ConsistentWith(MutationState);
            }
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var statement = GenerateQuery(queryModel, out var scalarResultBehavior);

            var queryOptions = new LinqQueryOptions(scalarResultBehavior);
            ApplyQueryOptionsSettings(queryOptions);

            return ExecuteCollection<T>(statement, queryOptions);
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

            var queryOptions = new LinqQueryOptions(scalarResultBehavior);
            ApplyQueryOptionsSettings(queryOptions);

            queryOptions.CancellationToken(cancellationToken);

            return ExecuteCollectionAsync<T>(statement, queryOptions);
        }

        /// <summary>
        /// Asynchronously execute a <see cref="LinqQueryOptions"/>.
        /// </summary>
        /// <typeparam name="T">Type returned by the query.</typeparam>
        /// <param name="statement">Query to execute.</param>
        /// <param name="queryOptions">Options to control execution.</param>
        /// <returns>Task which contains a list of objects returned by the request when complete.</returns>
        public async IAsyncEnumerable<T> ExecuteCollectionAsync<T>(string statement, LinqQueryOptions queryOptions)
        {
            // TODO: Make this more efficient with a custom enumerator

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

            await foreach (var row in result.ConfigureAwait(false))
            {
                yield return row;
            }
        }

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            return ExecuteSingle<T>(queryModel, false);
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            var result = returnDefaultWhenEmpty
                ? ExecuteCollection<T>(queryModel).SingleOrDefault()
                : ExecuteCollection<T>(queryModel).Single();

            return result;
        }

        public string GenerateQuery(QueryModel queryModel, out ScalarResultBehavior scalarResultBehavior)
        {
            // If ITypeSerializer is an IExtendedTypeSerializer, use it as the member name resolver
            // Otherwise fallback to the legacy behavior which assumes we're using Newtonsoft.Json
            // Note that DefaultSerializer implements IExtendedTypeSerializer, but has the same logic as JsonNetMemberNameResolver

            var serializer = Serializer as IExtendedTypeSerializer;

#pragma warning disable CS0618 // Type or member is obsolete
            var memberNameResolver = serializer != null ?
                (IMemberNameResolver)new ExtendedTypeSerializerMemberNameResolver(serializer) :
                (IMemberNameResolver)new JsonNetMemberNameResolver(JsonConvert.DefaultSettings().ContractResolver);
#pragma warning restore CS0618 // Type or member is obsolete

            var methodCallTranslatorProvider = new DefaultMethodCallTranslatorProvider();

            var queryGenerationContext = new N1QlQueryGenerationContext
            {
                MemberNameResolver = memberNameResolver,
                MethodCallTranslatorProvider = methodCallTranslatorProvider,
                Serializer = serializer,
                // TODO: Don't use .Result
                ClusterVersion = _clusterVersionProvider.GetVersionAsync().Result ?? FeatureVersions.DefaultVersion,
                LoggerFactory = _cluster.ClusterServices.GetRequiredService<ILoggerFactory>()
            };

            var visitor = new N1QlQueryModelVisitor(queryGenerationContext);
            visitor.VisitQueryModel(queryModel);

            var query = visitor.GetQuery();
            _logger.LogDebug("Generated query: {0}", query);

            scalarResultBehavior = visitor.ScalarResultBehavior;
            return query;
        }
    }
}