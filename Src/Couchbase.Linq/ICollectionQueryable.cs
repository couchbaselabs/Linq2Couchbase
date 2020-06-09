using System.Collections.Generic;
using System.Linq;

namespace Couchbase.Linq
{
    /// <summary>
    /// IQueryable sourced from a Couchbase collection.  Used to provide the collection name to the query generator.
    /// </summary>
    public interface ICollectionQueryable
    {
        /// <summary>
        /// Collection query is run against
        /// </summary>
        string CollectionName { get; }

        /// <summary>
        /// Scope query is run against
        /// </summary>
        string ScopeName { get; }

        /// <summary>
        /// Bucket query is run against
        /// </summary>
        string BucketName { get; }
    }

    /// <summary>
    /// IQueryable sourced from a Couchbase collection.  Used to provide the collection name to the query generator.
    /// </summary>
    public interface ICollectionQueryable<out T> : IQueryable<T>, ICollectionQueryable, IAsyncEnumerable<T>
    {
    }
}
