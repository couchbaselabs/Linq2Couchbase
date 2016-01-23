using System.Linq;
using Couchbase.Configuration.Client;
using Couchbase.Core;

namespace Couchbase.Linq
{
    /// <summary>
    /// Provides a single point of entry to a Couchbase bucket which makes it easier to compose
    /// and execute queries and to group togather changes which will be submitted back into the bucket.
    /// </summary>
    public interface IBucketContext : IBucketQueryable
    {
        /// <summary>
        /// Gets the configuration for the current <see cref="Cluster"/>.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        ClientConfiguration Configuration { get; }

        /// <summary>
        /// If true, generate change tracking proxies for documents during deserialization. Defaults to false for higher performance queries.
        /// </summary>
        bool EnableChangeTracking { get; set; }

        /// <summary>
        /// Begins change tracking for the current request. To complete and save the changes call <see cref="SubmitChanges"/>. Note that
        /// <see cref="EnableChangeTracking"/> must be set to true for change tracking to be enabled.
        /// </summary>
        void BeginChangeTracking();

        /// <summary>
        /// Submits the changes.
        /// </summary>
        void SubmitChanges();

        /// <summary>
        /// Flushes the changes.
        /// </summary>
        void FlushChanges();

        /// <summary>
        /// Queries the current <see cref="IBucket" /> for entities of type T. This is the target of
        /// a LINQ query and requires that the associated JSON document have a type property that is the same as T.
        /// </summary>
        /// <typeparam name="T">An entity or POCO representing the object graph of a JSON document.</typeparam>
        /// <returns><see cref="IQueryable{T}" /> which can be used to query the bucket.</returns>
        IQueryable<T> Query<T>();

        /// <summary>
        /// Saves the specified document.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document">The document.</param>
        void Save<T>(T document);

        /// <summary>
        /// Removes the specified document.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document">The document.</param>
        void Remove<T>(T document);
    }
}
