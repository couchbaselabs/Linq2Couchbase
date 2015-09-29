using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Couchbase.Core;
using Couchbase.Linq.QueryGeneration;
using Couchbase.N1QL;
using Newtonsoft.Json;
using Remotion.Linq;
using Remotion.Linq.Clauses.ResultOperators;

namespace Couchbase.Linq
{
    public class BucketQueryExecutor : IQueryExecutor
    {
        private static readonly ILog Log = LogManager.GetLogger<BucketQueryExecutor>();
        private readonly IBucket _bucket;

        public string BucketName
        {
            get { return _bucket.Name; }
        }

        public BucketQueryExecutor(IBucket bucket)
        {
            _bucket = bucket;
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var commandData = ExecuteCollection(queryModel);
            var result = _bucket.Query<T>(new QueryRequest(commandData));
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
                var result = ExecuteSingle<AnyAllResult>(queryModel, true);

                // For an Any operation, no result row means that the Any should return false
                return (T)(object)(result != null ? result.result : false);
            }
            else if (queryModel.ResultOperators.Any(p => p is AllResultOperator))
            {
                // Need to extract the value from an object
                var result = ExecuteSingle<AnyAllResult>(queryModel, true);

                // For an All operation, no result row means that the All should return true
                return (T)(object)(result != null ? result.result : true);
            }

            return ExecuteCollection<T>(queryModel).Single();
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            return returnDefaultWhenEmpty
                ? ExecuteCollection<T>(queryModel).SingleOrDefault()
                : ExecuteCollection<T>(queryModel).Single();
        }

        public string ExecuteCollection(QueryModel queryModel)
        {
            var query = N1QlQueryModelVisitor.GenerateN1QlQuery(queryModel);

            Log.Debug(m => m("Generated query: {0}", query));

            return query;
        }

        /// <summary>
        /// Used to extract the result row from an Any or All operation
        /// </summary>
        private class AnyAllResult
        {
            public bool result { get; set; }
        }

    }
}