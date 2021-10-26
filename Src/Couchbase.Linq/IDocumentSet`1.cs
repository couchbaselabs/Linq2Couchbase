using System;
using System.Linq;
using Couchbase.KeyValue;

namespace Couchbase.Linq
{
    /// <summary>
    /// A set of documents in a Couchbase collection.
    /// </summary>
    /// <typeparam name="T">Type of the document.</typeparam>
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IDocumentSet<T> : IQueryable<T>
    {
        /// <summary>
        /// The couchbase collection for these documents.
        /// </summary>
        ICouchbaseCollection Collection { get; }
    }
}
