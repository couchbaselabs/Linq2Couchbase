using System.Linq;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.N1QL;

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
        /// Begins change tracking for the current request. To complete and save the changes call <see cref="SubmitChanges"/>.
        /// </summary>
        void BeginChangeTracking();

        /// <summary>
        /// Ends change tracking on the current context.
        /// </summary>
        void EndChangeTracking();

        /// <summary>
        /// Submits the changes.
        /// </summary>
        void SubmitChanges();

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

        /// <summary>
        /// If true, generate change tracking proxies for documents during deserialization. Defaults to false for higher performance queries.
        /// </summary>
        bool ChangeTrackingEnabled { get; }

        #region Mutation State

        /// <summary>
        /// The current <see cref="N1QL.MutationState"/>.  May return null if there
        /// have been no mutations.
        /// </summary>
        /// <remarks>
        /// This value is updated as mutations are applied via <see cref="Save{T}"/>.  It may be used
        /// to enable read-your-own-write by passing the value to <see cref="Extensions.QueryExtensions.ConsistentWith{T}"/>.
        /// If you are using change tracking, this value won't be valid until after a call to <see cref="SubmitChanges"/>.
        /// This function is only supported on Couchbase Server 4.5 or later.
        /// </remarks>
        MutationState MutationState { get; }

        /// <summary>
        /// Resets the <see cref="MutationState"/> to start a new set of mutations.
        /// </summary>
        /// <remarks>
        /// If you are using an <see cref="IBucketContext"/> over and extended period of time,
        /// performing a reset regularly is recommend.  This will help keep the size of the
        /// <see cref="MutationState"/> to a minimum.
        /// </remarks>
        void ResetMutationState();

        #endregion
    }
}
