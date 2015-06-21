using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Couchbase.Core;
using Couchbase.Linq.QueryGeneration;
using Couchbase.N1QL;
using Newtonsoft.Json;
using Remotion.Linq;

namespace Couchbase.Linq
{
    public class BucketQueryExecuter : IQueryExecutor
    {
        private readonly IBucket _bucket;

        public string BucketName
        {
            get { return _bucket.Name; }
        }

        public BucketQueryExecuter(IBucket bucket)
        {
            _bucket = bucket;
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var commandData = ExecuteCollection(queryModel);
            var result = _bucket.Query<T>(new QueryRequest(commandData));
            if (!result.Success)
            {
                if (result.Exception != null && result.Errors == null)
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
            return N1QlQueryModelVisitor.GenerateN1QlQuery(queryModel);
        }
    }
}