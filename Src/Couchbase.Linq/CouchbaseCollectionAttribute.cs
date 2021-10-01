using System;
using Couchbase.Linq.QueryGeneration;

#nullable enable

namespace Couchbase.Linq
{
    /// <summary>
    /// Annotates a document as belonging to a specific scope/collection. If not present, the default collection is assumed.
    /// </summary>
    /// <remarks>
    /// This may be applied to a document class or to a property implementing <see cref="IDocumentSet{T}"/> on a class
    /// inherited from <see cref="BucketContext"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CouchbaseCollectionAttribute : Attribute
    {
        internal static CouchbaseCollectionAttribute Default { get; } =
            new(N1QlHelpers.DefaultScopeName, N1QlHelpers.DefaultCollectionName);

        /// <summary>
        /// Name of the scope.
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// Name of the collection.
        /// </summary>
        public string Collection { get; set; }

        /// <summary>
        /// Create a new CouchbaseCollectionAttribute.
        /// </summary>
        /// <param name="scope">Name of the scope.</param>
        /// <param name="collection">Name of the collection.</param>
        public CouchbaseCollectionAttribute(string scope, string collection)
        {
            Scope = scope;
            Collection = collection;
        }

        /// <summary>
        /// Deconstruct into variables.
        /// </summary>
        /// <param name="scope">Name of the scope.</param>
        /// <param name="collection">Name of the collection.</param>
        public void Deconstruct(out string scope, out string collection)
        {
            scope = Scope;
            collection = Collection;
        }
    }
}
