    using System;

namespace Couchbase.Linq.Filters
{
    /// <summary>
    /// Generates an <see cref="DocumentFilterSet&lt;T&gt;">DocumentFilterSet</see> for a particular type.
    /// </summary>
    public interface IDocumentFilterSetGenerator
    {

        /// <summary>
        /// Generates an <see cref="DocumentFilterSet&lt;T&gt;">DocumentFilterSet</see> for a particular type.
        /// </summary>
        /// <returns>Returns null if there are no filters.  This is to improve efficieny.</returns>
        DocumentFilterSet<T> GenerateDocumentFilterSet<T>();

    }
}
