using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Linq.QueryGeneration;
using Remotion.Linq;

namespace Couchbase.Linq
{
    public class BucketQueryExecuter : IQueryExecutor
    {
        private readonly IBucket _bucket;

        public BucketQueryExecuter(IBucket bucket)
        {
            _bucket = bucket;
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var commandData = ExecuteCollection(queryModel);
            var result = _bucket.Query<T>(commandData);
            if (!result.Success)
            {
                if (result.Exception != null)
                {
                    throw result.Exception;
                }
                if (result.Error != null)
                {
                    throw new Exception(result.Error.Message);
                }
            }

            return result.Rows ?? new List<T>();//need to figure out how to return more data
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
            return N1QlQueryModelVisitor.GenerateN1QlQuery(queryModel, _bucket.Name);
        }
    }
}
