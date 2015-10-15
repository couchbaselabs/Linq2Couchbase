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
        /// Queries the current <see cref="IBucket"/> for entities of type <see cref="T"/>. This is the target of
        /// the Linq query requires that the associated JSON document have a type property that is the same as <see cref="T"/>.
        /// </summary>
        /// <typeparam name="T">An entity or POCO representing the object graph of a JSON document.</typeparam>
        /// <returns></returns>
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
