using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Core.Serialization;
using Couchbase.Linq.Operators;
using Couchbase.Linq.QueryGeneration;
using Couchbase.Linq.QueryGeneration.MemberNameResolvers;
using Couchbase.N1QL;
using Newtonsoft.Json;
using Remotion.Linq;
using Remotion.Linq.Clauses.ResultOperators;

namespace Couchbase.Linq
{
    internal class BucketQueryExecutor : IQueryExecutor
    {
        private static readonly ILog Log = LogManager.GetLogger<BucketQueryExecutor>();
        private readonly IBucket _bucket;
        private readonly ClientConfiguration _configuration;
        private readonly bool _enableProxyGeneration;

        public string BucketName
        {
            get { return _bucket.Name; }
        }

        /// <summary>
        /// If true, generate change tracking proxies for documents during deserialization.
        /// </summary>
        public bool EnableProxyGeneration
        {
            get { return _enableProxyGeneration; }
        }

        /// <summary>
        /// Creates a new BucketQueryExecutor.
        /// </summary>
        /// <param name="bucket"><see cref="IBucket"/> to query.</param>
        /// <param name="configuration"><see cref="ClientConfiguration"/> used during the query.</param>
        /// <param name="enableProxyGeneration">If true, generate change tracking proxies for documents during deserialization.</param>
        public BucketQueryExecutor(IBucket bucket, ClientConfiguration configuration, bool enableProxyGeneration)
        {
            _bucket = bucket;
            _configuration = configuration;
            _enableProxyGeneration = enableProxyGeneration;
        }

        /// <summary>
        /// Determines if proxies should be generated, based on the given query model and return type.
        /// </summary>
        /// <typeparam name="T">Return type expected for query rows.</typeparam>
        /// <param name="queryModel">Query model.</param>
        /// <returns>Returns true if proxies should be generated, based on the given query model and return type.</returns>
        /// <remarks>
        /// Queries with select projections don't need change tracking, because there is no original source document be
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

            return (mainFromClauseType == typeof (T)) || (mainFromClauseType == typeof (SimpleResult<T>));
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            bool resultExtractionRequired;

            var commandData = ExecuteCollection(queryModel, out resultExtractionRequired);

            if (!resultExtractionRequired)
            {
                return ExecuteCollection<T>(commandData, queryModel);
            }
            else
            {
                return ExecuteCollection<SimpleResult<T>>(commandData, queryModel)
                    .Select(p => p.result);
            }
        }

        private IEnumerable<T> ExecuteCollection<T>(string commandData, QueryModel queryModel)
        {
            var queryRequest = new QueryRequest(commandData);

            if (ShouldGenerateProxies<T>(queryModel))
            {
                // Proxy generation was requested, and the
                queryRequest.DataMapper = new Proxies.DocumentProxyDataMapper(_configuration);
            }

            var result = _bucket.Query<T>(queryRequest);
            if (!result.Success)
            {
                if (result.Exception != null && (result.Errors == null || result.Errors.Count == 0))
                {
                    throw result.Exception;
                }
                if (result.Errors != null)
                {
                    var sb = new StringBuilder();
                    foreach (var error in result.Errors)
                    {
                        sb.AppendLine(JsonConvert.SerializeObject(error));
                    }
                    throw new Exception(sb.ToString());
                }
            }

            return result.Rows ?? new List<T>(); //need to figure out how to return more data
        }

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            if (queryModel.ResultOperators.Any(p => p is AnyResultOperator))
            {
                // Need to extract the value from an object
                var result = ExecuteSingle<SimpleResult<bool>>(queryModel, true);

                // For an Any operation, no result row means that the Any should return false
                return (T)(object)(result != null ? result.result : false);
            }
            else if (queryModel.ResultOperators.Any(p => p is AllResultOperator))
            {
                // Need to extract the value from an object
                var result = ExecuteSingle<SimpleResult<bool>>(queryModel, true);

                // For an All operation, no result row means that the All should return true
                return (T)(object)(result != null ? result.result : true);
            }
            else if (queryModel.ResultOperators.Any(p => p is ExplainResultOperator))
            {
                return ExecuteSingle<T>(queryModel, false);
            }
            else
            {
                return ExecuteSingle<T>(queryModel, false);
            }
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            return returnDefaultWhenEmpty
                ? ExecuteCollection<T>(queryModel).SingleOrDefault()
                : ExecuteCollection<T>(queryModel).Single();
        }

        public string ExecuteCollection(QueryModel queryModel, out bool resultExtractionRequired)
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

            var visitor = new N1QlQueryModelVisitor(memberNameResolver, methodCallTranslatorProvider, _configuration.Serializer.Invoke());
            visitor.VisitQueryModel(queryModel);

            var query = visitor.GetQuery();
            Log.Debug(m => m("Generated query: {0}", query));

            resultExtractionRequired = visitor.ResultExtractionRequired;
            return query;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        /// <summary>
        /// Used to extract the result row from an Any or All operation
        /// </summary>
        private class SimpleResult<T>
        {
            // ReSharper disable once InconsistentNaming
            // Note: must be virtual to support change tracking for First/Single
            public virtual T result { get; set; }
        }

    }
}