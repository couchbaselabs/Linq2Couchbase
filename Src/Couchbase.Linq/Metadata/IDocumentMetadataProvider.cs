namespace Couchbase.Linq.Metadata
{
    /// <summary>
    /// Returns metadata about the document object the interface is attached to
    /// </summary>
    /// <remarks>
    /// Used by a unit test to fake the results of a META operation in a Linq2Couchbase query
    /// </remarks>
    /// <seealso cref="N1QlFunctions.Meta" />
    public interface IDocumentMetadataProvider
    {

        /// <summary>
        /// Get metadata about this document
        /// </summary>
        /// <returns>Metadata about this document</returns>
        DocumentMetadata? GetMetadata();

    }
}
