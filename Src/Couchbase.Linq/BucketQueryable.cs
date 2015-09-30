using System;
using System.Linq;
using System.Linq.Expressions;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace Couchbase.Linq
{
    internal class BucketQueryable<T> : QueryableBase<T>, IBucketQueryable<T>
    {
        private readonly IBucket _bucket;
        private readonly ClientConfiguration _configuration;

        public string BucketName
        {
            get { return _bucket.Name; }
        }

        public BucketQueryable(IBucket bucket, IQueryParser queryParser, IQueryExecutor executor)
            : base(queryParser, executor)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }

            _bucket = bucket;
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

        public BucketQueryable(IBucket bucket, ClientConfiguration configuration)
            : base(QueryParserHelper.CreateQueryParser(), new BucketQueryExecutor(bucket, configuration))
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }

            _bucket = bucket;
            _configuration = configuration;
        }
    }
}