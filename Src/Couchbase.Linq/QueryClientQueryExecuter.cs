using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.QueryGeneration;
using Remotion.Linq;
using Couchbase.N1QL;

namespace Couchbase.Linq
{
    public sealed class QueryClientQueryExecuter : IQueryExecutor
    {
        private readonly Uri _uri;
        private readonly string _bucketName;
        private readonly IQueryClient _queryClient;
        public QueryClientQueryExecuter(IQueryClient queryClient, string bucketName, Uri uri)
        {
            _queryClient = queryClient;
            _bucketName = bucketName;
            _uri = uri;
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var commandData = ExecuteCollection(queryModel);
            var result = _queryClient.Query<T>(_uri, commandData);
            return result.Rows;
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
            return N1QlQueryModelVisitor.GenerateN1QlQuery(queryModel, _bucketName);
        }
    }
}
