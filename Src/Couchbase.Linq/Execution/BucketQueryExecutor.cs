using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Core.Buckets;
using Couchbase.Core.Serialization;
using Couchbase.Linq.Operators;
using Couchbase.Linq.QueryGeneration;
using Couchbase.Linq.QueryGeneration.MemberNameResolvers;
using Couchbase.Linq.Utils;
using Couchbase.Linq.Versioning;
using Couchbase.Logging;
using Couchbase.N1QL;
using Newtonsoft.Json;
using Remotion.Linq;
using Remotion.Linq.Clauses.ResultOperators;

namespace Couchbase.Linq.Execution
{
    internal class BucketQueryExecutor : IBucketQueryExecutor
    {
        private readonly ILog _log = LogManager.GetLogger<BucketQueryExecutor>();
        private readonly IBucket _bucket;
        private readonly ClientConfiguration _configuration;
        private readonly IBucketContext _bucketContext;

        public string BucketName
        {
            get { return _bucket.Name; }
        }

        /// <summary>
        /// If true, generate change tracking proxies for documents during deserialization.
        /// </summary>
        public bool EnableProxyGeneration
        {
            get { return _bucketContext.ChangeTrackingEnabled; }
        }

        /// <summary>
        /// Specifies the consistency guarantee/constraint for index scanning.
        /// </summary>
        public ScanConsistency? ScanConsistency { get; set; }

        /// <summary>
        /// Specifies the maximum time the client is willing to wait for an index to catch up to the consistency requirement in the request.
        /// If an index has to catch up, and the time is exceed doing so, an error is returned.
        /// </summary>
        public TimeSpan? ScanWait { get; set; }

        public MutationState MutationState { get; private set; }

        /// <inheritdoc cref="IBucketQueryExecutor.UseStreaming"/>
        public bool UseStreaming { get; set; }

        /// <summary>
        /// Creates a new BucketQueryExecutor.
        /// </summary>
        /// <param name="bucket"><see cref="IBucket"/> to query.</param>
        /// <param name="configuration"><see cref="ClientConfiguration"/> used during the query.</param>
        /// <param name="bucketContext">The context object for tracking and managing changes to documents.</param>
        public BucketQueryExecutor(IBucket bucket, ClientConfiguration configuration, IBucketContext bucketContext)
        {
            _bucket = bucket;
            _configuration = configuration;
            _bucketContext = bucketContext;
        }

        /// <summary>
        /// Requires that the indexes but up to date with a <see cref="N1QL.MutationState"/> before the query is executed.
        /// </summary>
        /// <param name="state"><see cref="N1QL.MutationState"/> used for conistency controls.</param>
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

        /// <summary>
        /// Determines if proxies should be generated, based on the given query model and return type.
        /// </summary>
        /// <typeparam name="T">Return type expected for query rows.</typeparam>
        /// <param name="queryModel">Query model.</param>
        /// <returns>Returns true if proxies should be generated, based on the given query model and return type.</returns>
        /// <remarks>
        /// Queries with select projections don't need change tracking, because there is no original source document to be
        /// updated if their properties are changed.  So only create proxies if the rows being returned by the query are
        /// plain instances of the document type being queried, without select projections.
        /// </remarks>
        private bool ShouldGenerateProxies<T>(QueryModel queryModel)
        {
            if (!EnableProxyGeneration)
            {
                return false;
            }

            var mainFromClauseType = queryModel.MainFromClause.ItemType;

            return (mainFromClauseType == typeof (T)) || (mainFromClauseType == typeof (ScalarResult<T>));
        }

        private void ApplyQueryRequestSettings(LinqQueryRequest queryRequest, bool generateProxies)
        {
            if (ScanConsistency.HasValue)
            {
                queryRequest.ScanConsistency(ScanConsistency.Value);
            }
            if (ScanWait.HasValue)
            {
                queryRequest.ScanWait(ScanWait.Value);
            }
            if (MutationState != null)
            {
                queryRequest.ConsistentWith(MutationState);
            }

            if (UseStreaming)
            {
                if (generateProxies)
                {
                    _log.Info("Query result streaming is not supported with change tracking, streaming disabled");
                }
                else
                {
                    queryRequest.UseStreaming(true);
                }
            }
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            ScalarResultBehavior scalarResultBehavior;
            bool generateProxies = ShouldGenerateProxies<T>(queryModel);

            var commandData = GenerateQuery(queryModel, generateProxies, out scalarResultBehavior);

            var queryRequest = new LinqQueryRequest(commandData, scalarResultBehavior);
            ApplyQueryRequestSettings(queryRequest, generateProxies);

            if (generateProxies)
            {
                // Proxy generation was requested, and the
                queryRequest.DataMapper = new Proxies.DocumentProxyDataMapper<T>(_configuration, (IChangeTrackableContext)_bucketContext);
            }

            if (queryModel.ResultOperators.Any(p => p is ToQueryRequestResultOperator))
            {
                // ToQueryRequest was called on this LINQ query, so we should return the N1QL query in a QueryRequest object
                // The IEnumerable wrapper will be stripped by ExecuteSingle.

                return new[] { queryRequest } as IEnumerable<T>;
            }

            return ExecuteCollection<T>(queryRequest);
        }

        /// <summary>
        /// Execute a <see cref="LinqQueryRequest"/>.
        /// </summary>
        /// <typeparam name="T">Type returned by the query.</typeparam>
        /// <param name="queryRequest">Request to execute.</param>
        /// <returns>List of objects returned by the request.</returns>
        public IEnumerable<T> ExecuteCollection<T>(LinqQueryRequest queryRequest)
        {
            if (!queryRequest.ScalarResultBehavior.ResultExtractionRequired)
            {
                var result = _bucket.Query<T>(queryRequest);

                return ParseResult(result);
            }
            else
            {
                var result = _bucket.Query<ScalarResult<T>>(queryRequest);

                return queryRequest.ScalarResultBehavior.ApplyResultExtraction(ParseResult(result));
            }
        }

        /// <summary>
        /// Asynchronously execute a <see cref="LinqQueryRequest"/>.
        /// </summary>
        /// <typeparam name="T">Type returned by the query.</typeparam>
        /// <param name="queryRequest">Request to execute.</param>
        /// <returns>Task which contains a list of objects returned by the request when complete.</returns>
        public async Task<IEnumerable<T>> ExecuteCollectionAsync<T>(LinqQueryRequest queryRequest)
        {
            if (!queryRequest.ScalarResultBehavior.ResultExtractionRequired)
            {
                var result = await _bucket.QueryAsync<T>(queryRequest).ConfigureAwait(false);

                return ParseResult(result);
            }
            else
            {
                var result = await _bucket.QueryAsync<ScalarResult<T>>(queryRequest).ConfigureAwait(false);

                return queryRequest.ScalarResultBehavior.ApplyResultExtraction(ParseResult(result));
            }
        }

        /// <summary>
        /// Parses a <see cref="IQueryResult{T}"/>, returning the result rows.
        /// If there are any errors, throws exceptions instead.
        /// </summary>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="result">Result to parse.</param>
        /// <returns>Rows in the result.</returns>
        private IEnumerable<T> ParseResult<T>(IQueryResult<T> result)
        {
            if (!result.Success)
            {
                if (result.Errors != null && (result.Errors.Count > 0))
                {
                    var message = result.Errors.Count == 1 ?
                        result.Errors[0].Message :
                        ExceptionMsgs.QueryExecutionMultipleErrors;

                    throw new CouchbaseQueryException(message ?? ExceptionMsgs.QueryExecutionUnknownError, result.Errors);
                }
                else if (result.Exception != null)
                {
                    throw new CouchbaseQueryException(ExceptionMsgs.QueryExecutionException, result.Exception);
                }
                else
                {
                    throw new CouchbaseQueryException(ExceptionMsgs.QueryExecutionUnknownError);
                }
            }

            return result;
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

        public async Task<T> ExecuteSingleAsync<T>(LinqQueryRequest queryRequest)
        {
            var resultSet = await ExecuteCollectionAsync<T>(queryRequest);

            return queryRequest.ReturnDefaultWhenEmpty
                ? resultSet.SingleOrDefault()
                : resultSet.Single();
        }

        public string GenerateQuery(QueryModel queryModel, bool selectDocumentMetadata, out ScalarResultBehavior scalarResultBehavior)
        {
            // If ITypeSerializer is an IExtendedTypeSerializer, use it as the member name resolver
            // Otherwise fallback to the legacy behavior which assumes we're using Newtonsoft.Json
            // Note that DefaultSerializer implements IExtendedTypeSerializer, but has the same logic as JsonNetMemberNameResolver

            var serializer = _configuration.Serializer.Invoke() as IExtendedTypeSerializer;

#pragma warning disable CS0618 // Type or member is obsolete
            var memberNameResolver = serializer != null ?
                (IMemberNameResolver)new ExtendedTypeSerializerMemberNameResolver(serializer) :
                (IMemberNameResolver)new JsonNetMemberNameResolver(_configuration.SerializationSettings.ContractResolver);
#pragma warning restore CS0618 // Type or member is obsolete

            var methodCallTranslatorProvider = new DefaultMethodCallTranslatorProvider();

            var queryGenerationContext = new N1QlQueryGenerationContext
            {
                MemberNameResolver = memberNameResolver,
                MethodCallTranslatorProvider = methodCallTranslatorProvider,
                Serializer = serializer,
                SelectDocumentMetadata = selectDocumentMetadata,
                ClusterVersion = VersionProvider.Current.GetVersion(_bucket)
            };

            var visitor = new N1QlQueryModelVisitor(queryGenerationContext);
            visitor.VisitQueryModel(queryModel);

            var query = visitor.GetQuery();
            _log.Debug("Generated query: {0}", query);

            scalarResultBehavior = visitor.ScalarResultBehavior;
            return query;
        }
    }
}