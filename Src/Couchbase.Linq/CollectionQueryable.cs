using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using Couchbase.KeyValue;
using Couchbase.Linq.Execution;
using Couchbase.Linq.QueryGeneration;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace Couchbase.Linq
{
    /// <summary>
    /// The main entry point and executor of the query.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CollectionQueryable<T> : QueryableBase<T>, ICollectionQueryable<T>
    {
        private readonly ICouchbaseCollection? _collection;

        /// <inheritdoc />
        public string CollectionName => _collection?.Name ?? N1QlHelpers.DefaultCollectionName;


        /// <inheritdoc />
        public string ScopeName => _collection?.Scope.Name ?? N1QlHelpers.DefaultScopeName;


        /// <inheritdoc />
        public string BucketName => _collection?.Scope.Bucket.Name ?? "";

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionQueryable{T}"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="queryParser">The query parser.</param>
        /// <param name="executor">The executor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="collection" /> is <see langword="null" />.</exception>
        public CollectionQueryable(ICouchbaseCollection collection, IQueryParser queryParser, IAsyncQueryExecutor executor)
            : base(new ClusterQueryProvider(queryParser, executor))
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionQueryable{T}"/> class.
        /// </summary>
        /// <remarks>Used to build new expressions as more methods are applied to the query.</remarks>
        /// <param name="provider">The provider.</param>
        /// <param name="expression">The expression.</param>
        public CollectionQueryable(IAsyncQueryProvider provider, Expression expression)
            : base(provider, expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionQueryable{T}"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="queryTimeout">Query timeout, if null uses cluster default.</param>
        public CollectionQueryable(ICouchbaseCollection collection, TimeSpan? queryTimeout)
            : this(collection,
                QueryParserHelper.CreateQueryParser(collection.Scope.Bucket.Cluster),
                new ClusterQueryExecutor(collection.Scope.Bucket.Cluster)
                {
                    QueryTimeout = queryTimeout
                })
        {
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            ((IAsyncQueryProvider) Provider).ExecuteAsync<IAsyncEnumerable<T>>(Expression)
                .GetAsyncEnumerator(cancellationToken);
    }
}