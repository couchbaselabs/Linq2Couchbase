using System.Linq.Expressions;
using Couchbase.Linq.Metadata;

namespace Couchbase.Linq
{
    /// <summary>
    /// Implements static helper methods for N1QL queries 
    /// </summary>
    public static class N1Ql
    {

        /// <summary>
        /// Returns metadata for a document object
        /// </summary>
        /// <param name="document">Document to get metadata from</param>
        /// <returns>Metadata about the document</returns>
        /// <remarks>Should only be called against a top-level document in Couchbase</remarks>
        public static DocumentMetadata Meta(object document)
        {
            // Implementation will only be called when unit testing
            // using LINQ-to-Objects and faking a Couchbase database
            // Any faked document object should implement IDocumentMetadataProvider

            var provider = document as IDocumentMetadataProvider;
            if (provider != null)
            {
                return provider.GetMetadata();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the key for a document object
        /// </summary>
        /// <param name="document">Document to get key from</param>
        /// <returns>Key of the document</returns>
        /// <remarks>Should only be called against a top-level document in Couchbase</remarks>
        public static string Key(object document)
        {
            // Implementation will only be called when unit testing
            // using LINQ-to-Objects and faking a Couchbase database
            // Any faked document object should implement IDocumentMetadataProvider

            var provider = document as IDocumentMetadataProvider;
            if (provider != null)
            {
                var metadata = provider.GetMetadata();

                return metadata != null ? metadata.Id : null;
            }
            else
            {
                return null;
            }
        }

        #region IsMissing and IsNotMissing

        /// <summary>
        /// Returns true if the selected property is missing from the document
        /// </summary>
        /// <typeparam name="T">Type of the property being selected</typeparam>
        /// <param name="property">Property to test</param>
        /// <returns>True if the property is missing from the document</returns>
        public static bool IsMissing<T>(T property)
        {
            // Implementation will only be called when unit testing
            // Since properties cannot be missing on in-memory objects
            // Always returns false

            return false;
        }

        /// <summary>
        /// Returns true if the named property is missing from the document
        /// </summary>
        /// <typeparam name="T">Type of the document being tested</typeparam>
        /// <param name="document">Document being tested</param>
        /// <param name="propertyName">Property name to test</param>
        /// <returns>True if the property is missing from the document</returns>
        /// <remarks><see cref="propertyName">propertyName</see> must be a constant when used in a LINQ expression</remarks>
        public static bool IsMissing<T>(T document, string propertyName)
        {
            // Implementation will only be called when unit testing
            // Test to see if the property is present via reflection

            return (document == null) || (typeof (T).GetProperty(propertyName) == null);
        }

        /// <summary>
        /// Returns true if the selected property is present on the document
        /// </summary>
        /// <typeparam name="T">Type of the property being selected</typeparam>
        /// <param name="property">Property to test</param>
        /// <returns>True if the property is present on the document</returns>
        public static bool IsNotMissing<T>(T property)
        {
            // Implementation will only be called when unit testing
            // Since properties cannot be missing on in-memory objects
            // Always returns true

            return true;
        }

        /// <summary>
        /// Returns true if the named property is present on the document
        /// </summary>
        /// <typeparam name="T">Type of the document being tested</typeparam>
        /// <param name="document">Document being tested</param>
        /// <param name="propertyName">Property name to test</param>
        /// <returns>True if the property is present on the document</returns>
        /// <remarks><see cref="propertyName">propertyName</see> must be a constant when used in a LINQ expression</remarks>
        public static bool IsNotMissing<T>(T document, string propertyName)
        {
            // Implementation will only be called when unit testing
            // Test to see if the property is present via reflection

            return (document != null) && (typeof(T).GetProperty(propertyName) != null);
        }

        #endregion

        #region IsValued and IsNotValued

        /// <summary>
        /// Returns true if the selected property is present on the document and not null
        /// </summary>
        /// <typeparam name="T">Type of the property being selected</typeparam>
        /// <param name="property">Property to test</param>
        /// <returns>True if the property is present on the document and not null</returns>
        public static bool IsValued<T>(T property)
        {
            // Implementation will only be called when unit testing
            // Since properties cannot be missing on in-memory objects
            // Simply test for null

            return property != null;
        }

        /// <summary>
        /// Returns true if the named property is not missing from the document and not null
        /// </summary>
        /// <typeparam name="T">Type of the document being tested</typeparam>
        /// <param name="document">Document being tested</param>
        /// <param name="propertyName">Property name to test</param>
        /// <returns>True if the property is present on the document and not null</returns>
        /// <remarks><see cref="propertyName">propertyName</see> must be a constant when used in a LINQ expression</remarks>
        public static bool IsValued<T>(T document, string propertyName)
        {
            // Implementation will only be called when unit testing
            // Test to see if the property is present and not null via reflection

            if (document == null)
            {
                return false;
            }

            var property = typeof (T).GetProperty(propertyName);
            if (property == null)
            {
                return false;
            }

            return property.GetValue(document) != null;
        }

        /// <summary>
        /// Returns true if the selected property is missing from the document or null
        /// </summary>
        /// <typeparam name="T">Type of the property being selected</typeparam>
        /// <param name="property">Property to test</param>
        /// <returns>True if the property is missing from the document or null</returns>
        public static bool IsNotValued<T>(T property)
        {
            // Implementation will only be called when unit testing
            // Since properties cannot be missing on in-memory objects
            // Simply test for null

            return property == null;
        }

        /// <summary>
        /// Returns true if the named property is missing from the document or null
        /// </summary>
        /// <typeparam name="T">Type of the document being tested</typeparam>
        /// <param name="document">Document being tested</param>
        /// <param name="propertyName">Property name to test</param>
        /// <returns>True if the property is missing from the document or null</returns>
        /// <remarks><see cref="propertyName">propertyName</see> must be a constant when used in a LINQ expression</remarks>
        public static bool IsNotValued<T>(T document, string propertyName)
        {
            // Implementation will only be called when unit testing
            // Test to see if the property is present via reflection

            if (document == null)
            {
                return true;
            }

            var property = typeof(T).GetProperty(propertyName);
            if (property == null)
            {
                return true;
            }

            return property.GetValue(document) == null;
        }

        #endregion

    }
}
