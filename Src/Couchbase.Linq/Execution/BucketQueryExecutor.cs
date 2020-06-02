using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        private ITypeSerializer _serializer;

        public string BucketName
        {
            get { return _bucket.Name; }
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

        /// <summary>
        /// Specifies the maximum time the server should wait for the QueryRequest to execute.
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        public MutationState MutationState { get; private set; }

        /// <inheritdoc cref="IBucketQueryExecutor.UseStreaming"/>
        public bool UseStreaming { get; set; }

        private ITypeSerializer Serializer
        {
            get
            {
                if (_serializer == null)
                {
                    var serializerProvider = _bucketContext.Bucket as ITypeSerializerProvider;

                    _serializer = serializerProvider?.Serializer ?? _configuration.Serializer.Invoke();
                }

                return _serializer;
            }
        }

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

        private void ApplyQueryRequestSettings(LinqQueryRequest queryRequest)
        {
            if (ScanConsistency.HasValue)
            {
                queryRequest.ScanConsistency(ScanConsistency.Value);
            }
            if (ScanWait.HasValue)
            {
                queryRequest.ScanWait(ScanWait.Value);
            }
            if (Timeout.HasValue)
            {
                queryRequest.Timeout(Timeout.Value);
            }
            if (MutationState != null)
            {
                queryRequest.ConsistentWith(MutationState);
            }

            if (UseStreaming)
            {
                queryRequest.UseStreaming(true);
            }
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            ScalarResultBehavior scalarResultBehavior;

            var commandData = GenerateQuery(queryModel, out scalarResultBehavior);

            var queryRequest = new LinqQueryRequest(commandData, scalarResultBehavior);
            ApplyQueryRequestSettings(queryRequest);

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
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task which contains a list of objects returned by the request when complete.</returns>
        public async Task<IEnumerable<T>> ExecuteCollectionAsync<T>(LinqQueryRequest queryRequest, CancellationToken cancellationToken)
        {
            if (!queryRequest.ScalarResultBehavior.ResultExtractionRequired)
            {
                var result = await _bucket.QueryAsync<T>(queryRequest, cancellationToken).ConfigureAwait(false);

                return ParseResult(result);
            }
            else
            {
                var result = await _bucket.QueryAsync<ScalarResult<T>>(queryRequest, cancellationToken).ConfigureAwait(false);

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

        public async Task<T> ExecuteSingleAsync<T>(LinqQueryRequest queryRequest, CancellationToken cancellationToken)
        {
            var resultSet = await ExecuteCollectionAsync<T>(queryRequest, cancellationToken).ConfigureAwait(false);

            return queryRequest.ReturnDefaultWhenEmpty
                ? resultSet.SingleOrDefault()
                : resultSet.Single();
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
                (IMemberNameResolver)new JsonNetMemberNameResolver(_configuration.SerializationSettings.ContractResolver);
#pragma warning restore CS0618 // Type or member is obsolete

            var methodCallTranslatorProvider = new DefaultMethodCallTranslatorProvider();

            var queryGenerationContext = new N1QlQueryGenerationContext
            {
                MemberNameResolver = memberNameResolver,
                MethodCallTranslatorProvider = methodCallTranslatorProvider,
                Serializer = serializer,
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