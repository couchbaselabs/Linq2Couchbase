using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Core.IO.Serializers;
using Couchbase.Core.Version;
using Couchbase.Linq.Clauses;
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

        private LinqQueryOptions GetQueryOptions(QueryModel queryModel, ScalarResultBehavior scalarResultBehavior)
        {
            var queryOptions = new LinqQueryOptions(scalarResultBehavior);

            MutationState combinedMutationState = null;

            foreach (var bodyClause in queryModel.BodyClauses)
            {
                switch (bodyClause)
                {
                    case ScanConsistencyClause scanConsistency:
                        queryOptions.ScanConsistency(scanConsistency.ScanConsistency);
                        break;

                    case ConsistentWithClause consistentWith:
                        combinedMutationState ??= new MutationState();
                        combinedMutationState.Add(consistentWith.MutationState);
                        break;
                }
            }

            if (combinedMutationState != null)
            {
                queryOptions.ConsistentWith(combinedMutationState);
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