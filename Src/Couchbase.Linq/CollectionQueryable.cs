﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Couchbase.KeyValue;
using Couchbase.Linq.Execution;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace Couchbase.Linq
{
    /// <summary>
    /// The main entry point and executor of the query.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CollectionQueryable<T> : QueryableBase<T>, ICollectionQueryable<T>, IAsyncEnumerable<T>,
        IClusterQueryExecutorProvider
    {
        private readonly ICouchbaseCollection _collection;

        /// <inheritdoc />
        public string CollectionName => _collection.Name;


        /// <inheritdoc />
        public string ScopeName => _collection.Scope.Name;


        /// <inheritdoc />
        public string BucketName => _collection.Scope.Bucket.Name;

        /// <summary>
        /// Get the <see cref="IClusterQueryExecutor"/>.
        /// </summary>
        public IClusterQueryExecutor ClusterQueryExecutor =>
            (IClusterQueryExecutor) ((ClusterQueryProvider) Provider).Executor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionQueryable{T}"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="queryParser">The query parser.</param>
        /// <param name="executor">The executor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="collection" /> is <see langword="null" />.</exception>
        public CollectionQueryable(ICouchbaseCollection collection, IQueryParser queryParser, IClusterQueryExecutor executor)
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
        public CollectionQueryable(ClusterQueryProvider provider, Expression expression)
            : base(provider, expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionQueryable{T}"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public CollectionQueryable(ICouchbaseCollection collection)
            : this(collection,
                QueryParserHelper.CreateQueryParser(collection.Scope.Bucket.Cluster),
                new ClusterQueryExecutor(collection.Scope.Bucket.Cluster))
        {
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            ((IAsyncQueryProvider) Provider).ExecuteAsync<IAsyncEnumerable<T>>(Expression)
                .GetAsyncEnumerator(cancellationToken);
    }
}