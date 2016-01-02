using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.Metadata;

namespace Couchbase.Linq
{
    public static partial class N1QlFunctions
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
    }
}
