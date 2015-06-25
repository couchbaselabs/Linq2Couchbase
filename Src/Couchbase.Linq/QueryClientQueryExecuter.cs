using System;
using System.Collections.Generic;
using System.Linq;
using Couchbase.Linq.QueryGeneration;
using Couchbase.N1QL;
using Remotion.Linq;

namespace Couchbase.Linq
{
    public sealed class QueryClientQueryExecuter : IQueryExecutor, IBucketQueryable
    {
        private readonly string _bucketName;
        private readonly IQueryClient _queryClient;
        private readonly Uri _uri;

        public string BucketName
        {
            get { return _bucketName; }
        }

        internal QueryClientQueryExecuter(IQueryClient queryClient, string bucketName, Uri uri)
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
            return N1QlQueryModelVisitor.GenerateN1QlQuery(queryModel);
        }
    }
}