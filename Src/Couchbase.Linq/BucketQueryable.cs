using System.Linq;
using System.Linq.Expressions;
using Couchbase.Core;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace Couchbase.Linq
{
    public class BucketQueryable<T> : QueryableBase<T>
    {
        public BucketQueryable(IQueryParser queryParser, IQueryExecutor executor)
            : base(queryParser, executor)
        {
        }

        public BucketQueryable(IQueryProvider provider)
            : base(provider)
        {
        }

        public BucketQueryable(IQueryProvider provider, Expression expression)
            : base(provider, expression)
        {
        }

        public BucketQueryable(IBucket bucket)
            : base(QueryParserHelper.CreateQueryParser(), new BucketQueryExecuter(bucket))
        {
        }
    }
}