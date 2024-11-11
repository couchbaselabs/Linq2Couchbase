using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Couchbase.KeyValue;
using Couchbase.Linq.Execution;
using Couchbase.Linq.Utils;

namespace Couchbase.Linq
{
    /// <summary>
    /// Default implementation of <see cref="IDocumentSet{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of the document.</typeparam>
    internal class DocumentSet<T> : IDocumentSet<T>, ICollectionQueryable<T>
    {
        private readonly IAsyncQueryProvider _queryProvider;

        /// <inheritdoc />
        public string BucketName { get; }

        /// <inheritdoc />
        public string ScopeName { get; }

        /// <inheritdoc />
        public string CollectionName { get; }

        /// <inheritdoc />
        public ICouchbaseCollection Collection { get; }

        public DocumentSet(BucketContext bucketContext, string scopeName, string collectionName)
        {
            ThrowHelpers.ThrowIfNull(bucketContext);
            ThrowHelpers.ThrowIfNull(scopeName);
            ThrowHelpers.ThrowIfNull(collectionName);

            _queryProvider = bucketContext.QueryProvider;
            Collection = bucketContext.Bucket.Scope(scopeName).Collection(collectionName);

            BucketName = bucketContext.Bucket.Name;
            ScopeName = scopeName;
            CollectionName = collectionName;

            // Note: For this to work, we must implement ICollectionQueryable
            Expression = Expression.Constant(this);
        }

        #region IQueryable

        private CouchbaseQueryable<T> MakeQueryable() => new(_queryProvider, Expression);

        public IEnumerator<T> GetEnumerator() => MakeQueryable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Type ElementType { get; } = typeof(T);
        public Expression Expression { get; }
        public IQueryProvider Provider => _queryProvider;

        #endregion

        #region IAsyncEnumerable

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            MakeQueryable().GetAsyncEnumerator(cancellationToken);

        #endregion
    }
}
