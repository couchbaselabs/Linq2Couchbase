using System.Linq;
using System.Linq.Expressions;
using Couchbase.Core;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace Couchbase.Linq
{
    public class BucketQueryable<T> : QueryableBase<T>, IBucketQueryable
    {

        private readonly IBucket _bucket;

        public string BucketName
        {
            get { return _bucket.Name; }
        }

        public BucketQueryable(IQueryParser queryParser, IQueryExecutor executor)
            : base(queryParser, executor)
        {
            // Is this constructor necessary?
        }

        public BucketQueryable(IQueryProvider provider)
            : base(provider)
        {
            // Is this constructor necessary?
        }

        public BucketQueryable(IQueryProvider provider, Expression expression)
            : base(provider, expression)
        {
            // Is this constructor necessary?
        }

        public BucketQueryable(IBucket bucket)
            : base(QueryParserHelper.CreateQueryParser(), new BucketQueryExecuter(bucket))
        {
            _bucket = bucket;
        }
    }
}