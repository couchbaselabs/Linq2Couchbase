using System.Linq;

namespace Couchbase.Linq
{
    /// <summary>
    /// IQueryable sourced from a Couchbase bucket.  Used to provide the bucket name to the query generator.
    /// </summary>
    public interface IBucketQueryable
    {
        /// <summary>
        /// Bucket query is run against
        /// </summary>
        string BucketName { get; }
    }

    /// <summary>
    /// IQueryable sourced from a Couchbase bucket.  Used to provide the bucket name to the query generator.
    /// </summary>
    public interface IBucketQueryable<out T> : IQueryable<T>, IBucketQueryable
    {
    }
}
