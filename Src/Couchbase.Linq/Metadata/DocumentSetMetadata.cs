using System;
using System.Reflection;

namespace Couchbase.Linq.Metadata
{
    /// <summary>
    /// Metadata about a specific property returning <see cref="IDocumentSet{T}"/> present on
    /// a class inherited from <see cref="BucketContext"/>.
    /// </summary>
    internal class DocumentSetMetadata
    {
        /// <summary>
        /// The property.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// The type of document.
        /// </summary>
        public Type DocumentType { get; set; }

        /// <summary>
        /// The collection where the document resides.
        /// </summary>
        public CouchbaseCollectionAttribute CollectionInfo { get; }

        public DocumentSetMetadata(PropertyInfo property)
        {
            Property = property;
            DocumentType = GetDocumentType(property.PropertyType);

            CollectionInfo = property.GetCustomAttribute<CouchbaseCollectionAttribute>()
                             ?? DocumentType.GetCustomAttribute<CouchbaseCollectionAttribute>()
                             ?? CouchbaseCollectionAttribute.Default;
        }

        private static Type GetDocumentType(Type propertyType) =>
            propertyType.GenericTypeArguments[0];
    }
}
