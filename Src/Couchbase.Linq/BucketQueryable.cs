using System;
using System.Linq;
using System.Linq.Expressions;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Linq.Execution;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace Couchbase.Linq
{
    /// <summary>
    /// The main entry point and executor of the query.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class BucketQueryable<T> : QueryableBase<T>, IBucketQueryable<T>, IBucketQueryExecutorProvider
    {
        private readonly IBucket _bucket;
        private readonly IBucketQueryExecutor _bucketQueryExecutor;
        private readonly IBucketContext _bucketContext;

        /// <summary>
        /// Bucket query is run against
        /// </summary>
        public string BucketName
        {
            get { return _bucket.Name; }
        }

        /// <summary>
        /// Get the <see cref="IBucketQueryExecutor"/>.
        /// </summary>
        public IBucketQueryExecutor BucketQueryExecutor
        {
            get { return _bucketQueryExecutor; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BucketQueryable{T}"/> class.
        /// </summary>
        /// <param name="bucket">The bucket.</param>
        /// <param name="queryParser">The query parser.</param>
        /// <param name="executor">The executor.</param>
        /// <exception cref="System.ArgumentNullException">bucket</exception>
        /// <exception cref="ArgumentNullException"><paramref name="bucket" /> is <see langword="null" />.</exception>
        public BucketQueryable(IBucket bucket, IQueryParser queryParser, IBucketQueryExecutor executor)
            : base(queryParser, executor)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }

            _bucket = bucket;
            _bucketQueryExecutor = executor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BucketQueryable{T}"/> class.
        /// </summary>
        /// <remarks>Used to build new expressions as more methods are applied to the query.</remarks>
        /// <param name="provider">The provider.</param>
        /// <param name="expression">The expression.</param>
        public BucketQueryable(IQueryProvider provider, Expression expression)
            : base(provider, expression)
        {
            _bucketQueryExecutor = (IBucketQueryExecutor) ((DefaultQueryProvider) provider).Executor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BucketQueryable{T}"/> class.
        /// </summary>
        /// <param name="bucket">The bucket.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="bucketContext">The context object for tracking and managing changes to documents.</param>
        /// <exception cref="System.ArgumentNullException">bucket</exception>
        /// <exception cref="ArgumentNullException"><paramref name="bucket" /> is <see langword="null" />.</exception>
        public BucketQueryable(IBucket bucket, ClientConfiguration configuration, IBucketContext bucketContext)
            : base(QueryParserHelper.CreateQueryParser(), new BucketQueryExecutor(bucket, configuration, bucketContext))
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }

            _bucket = bucket;
            _bucketQueryExecutor = (IBucketQueryExecutor) ((DefaultQueryProvider) Provider).Executor;
            _bucketContext = bucketContext;
        }
    }
}