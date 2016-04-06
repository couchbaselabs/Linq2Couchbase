using System;
using Couchbase.Linq.Proxies;

namespace Couchbase.Linq
{
    /// <summary>
    /// Provides an interface for supporting persistence of documents via proxies when change tracking is enabled.
    /// </summary>
    internal interface IChangeTrackableContext : ITrackedDocumentNodeCallback
    {
        /// <summary>
        /// Adds a document to the list of tracked documents if change tracking is enabled.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the document to track.</typeparam>
        /// <param name="document">The object representing the document.</param>
        void Track<T>(T document);

        /// <summary>
        /// Removes a document from the list if tracked documents.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document"></param>
        void Untrack<T>(T document);

        /// <summary>
        /// Adds a document to the list of modified documents if it is has been mutated and is being tracked.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document"></param>
        void Modified<T>(T document);

        /// <summary>
        /// The count of documents that have been modified if change tracking is enabled.
        /// </summary>
        int ModifiedCount { get; }

        /// <summary>
        /// The count of all documents currently being tracked.
        /// </summary>
        int TrackedCount { get; }
    }
}
