#nullable enable

namespace Couchbase.Linq.Filters
{
    /// <summary>
    /// Generates a <see cref="DocumentFilterSet{T}" /> for a particular type.
    /// </summary>
    public interface IDocumentFilterSetGenerator
    {
        /// <summary>
        /// Generates a <see cref="DocumentFilterSet{T}" /> for a particular type.
        /// </summary>
        /// <returns>Returns null if there are no filters.  This is to improve efficiency.</returns>
        DocumentFilterSet<T>? GenerateDocumentFilterSet<T>();
    }
}
