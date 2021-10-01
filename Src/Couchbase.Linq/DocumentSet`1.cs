using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Couchbase.KeyValue;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Utils;

namespace Couchbase.Linq
{
    /// <summary>
    /// Default implementation of <see cref="IDocumentSet{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of the document.</typeparam>
    internal class DocumentSet<T> : IDocumentSet<T>, ICollectionQueryable<T>
    {
        private readonly BucketContext _bucketContext;

        /// <inheritdoc />
        public string BucketName => _bucketContext.Bucket.Name;

        /// <inheritdoc />
        public string ScopeName { get; }

        /// <inheritdoc />
        public string CollectionName { get; }

        /// <inheritdoc />
        public ICouchbaseCollection Collection => _bucketContext.Bucket.Scope(ScopeName).Collection(CollectionName);

        public DocumentSet(BucketContext bucketContext, string scopeName, string collectionName)
        {
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if (bucketContext == null)
            {
                ThrowHelpers.ThrowArgumentNullException(nameof(bucketContext));
            }
            if (scopeName == null)
            {
                ThrowHelpers.ThrowArgumentNullException(nameof(scopeName));
            }
            if (collectionName == null)
            {
                ThrowHelpers.ThrowArgumentNullException(nameof(collectionName));
            }
            // ReSharper restore ConditionIsAlwaysTrueOrFalse

            _bucketContext = bucketContext;
            ScopeName = scopeName;
            CollectionName = collectionName;

            // Note: For this to work, we must implement ICollectionQueryable
            Expression = Expression.Constant(this);
        }

        /// <summary>
        /// Makes a new queryable for each query. This way the latest settings, such as timeout, are
        /// collected.
        /// </summary>
        private IQueryable<T> MakeQueryable() =>
            _bucketContext.Query<T>(ScopeName, CollectionName);

        #region IQueryable

        public IEnumerator<T> GetEnumerator() => MakeQueryable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Type ElementType => typeof(T);
        public Expression Expression { get; }
        public IQueryProvider Provider => MakeQueryable().Provider;

        #endregion

        #region IAsyncEnumerable

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            MakeQueryable().AsAsyncEnumerable().GetAsyncEnumerator(cancellationToken);

        #endregion
    }
}
