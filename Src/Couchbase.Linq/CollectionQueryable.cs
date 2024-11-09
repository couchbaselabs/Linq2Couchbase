using Couchbase.KeyValue;
using Couchbase.Linq.Execution;
using Couchbase.Linq.Utils;

namespace Couchbase.Linq
{
    /// <summary>
    /// The main entry point and executor of the query.
    /// </summary>
    /// <typeparam name="T">Document type to query.</typeparam>
    internal sealed class CollectionQueryable<T> : CouchbaseQueryable<T>, ICollectionQueryable<T>
    {
        /// <inheritdoc />
        public string CollectionName { get; }

        /// <inheritdoc />
        public string ScopeName { get; }

        /// <inheritdoc />
        public string BucketName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionQueryable{T}"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="provider">The query provider to execute the query.</param>
        public CollectionQueryable(ICouchbaseCollection collection, IAsyncQueryProvider provider) : base(provider)
        {
            ThrowHelpers.ThrowIfNull(collection);

            CollectionName = collection.Name;

            var scope = collection.Scope;
            ScopeName = scope.Name;
            BucketName = scope.Bucket.Name;
        }
    }
}