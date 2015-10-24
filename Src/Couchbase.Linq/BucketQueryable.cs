using System;
using System.Linq;
using System.Linq.Expressions;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace Couchbase.Linq
{
    /// <summary>
    /// The main entry point and executor of the query.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class BucketQueryable<T> : QueryableBase<T>, IBucketQueryable<T>
    {
        private readonly IBucket _bucket;

        /// <summary>
        /// Bucket query is run against
        /// </summary>
        public string BucketName
        {
            get { return _bucket.Name; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BucketQueryable{T}"/> class.
        /// </summary>
        /// <param name="bucket">The bucket.</param>
        /// <param name="queryParser">The query parser.</param>
        /// <param name="executor">The executor.</param>
        /// <exception cref="System.ArgumentNullException">bucket</exception>
        /// <exception cref="ArgumentNullException"><paramref name="bucket" /> is <see langword="null" />.</exception>
        public BucketQueryable(IBucket bucket, IQueryParser queryParser, IQueryExecutor executor)
            : base(queryParser, executor)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }
            _bucket = bucket;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BucketQueryable{T}"/> class.
        /// </summary>
        /// <remarks>Used by test project.</remarks>
        /// <param name="provider">The provider.</param>
        /// <param name="expression">The expression.</param>
        public BucketQueryable(IQueryProvider provider, Expression expression)
            : base(provider, expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BucketQueryable{T}"/> class.
        /// </summary>
        /// <param name="bucket">The bucket.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="enableProxyGeneration">If true, generate change tracking proxies for documents during deserialization.</param>
        /// <exception cref="System.ArgumentNullException">bucket</exception>
        /// <exception cref="ArgumentNullException"><paramref name="bucket" /> is <see langword="null" />.</exception>
        public BucketQueryable(IBucket bucket, ClientConfiguration configuration, bool enableProxyGeneration)
            : base(QueryParserHelper.CreateQueryParser(), new BucketQueryExecutor(bucket, configuration, enableProxyGeneration))
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }
            _bucket = bucket;
        }
    }
}