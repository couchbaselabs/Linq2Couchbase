using System.Linq;
using System.Reflection;

namespace Couchbase.Linq.Filters
{
    /// <summary>
    /// Generates a <see cref="DocumentFilterSet{T}" /> for a particular type, using <see cref="DocumentFilterAttribute" />.
    /// </summary>
    public class AttributeDocumentFilterSetGenerator : IDocumentFilterSetGenerator
    {
        /// <summary>
        /// Generates a <see cref="DocumentFilterSet{T}" /> for a particular type, using <see cref="DocumentFilterAttribute" />.
        /// </summary>
        /// <returns>Returns null if there are no filters. This is to improve efficiency.</returns>
        public DocumentFilterSet<T>? GenerateDocumentFilterSet<T>()
        {
            var filters = typeof(T).GetTypeInfo().GetCustomAttributes<DocumentFilterAttribute>(true).ToArray();

            return filters.Length > 0
                ? new DocumentFilterSet<T>(filters.Select(p => p.CreateFilter<T>()))
                : null;
        }

    }
}
