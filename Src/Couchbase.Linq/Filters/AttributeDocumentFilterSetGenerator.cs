using System;
using System.Linq;
using System.Reflection;

namespace Couchbase.Linq.Filters
{
    /// <summary>
    /// Generates an <see cref="DocumentFilterSet{T}">DocumentFilterSet</see> for a particular type, using <see cref="DocumentFilterAttribute">DocumentFilterAttributes</see>
    /// </summary>
    public class AttributeDocumentFilterSetGenerator : IDocumentFilterSetGenerator
    {

        /// <summary>
        /// Generates an <see cref="DocumentFilterSet{T}">DocumentFilterSet</see> for a particular type, using <see cref="DocumentFilterAttribute">DocumentFilterAttribute</see>s
        /// </summary>
        /// <returns>Returns null if there are no filters.  This is to improve efficieny.</returns>
        public DocumentFilterSet<T> GenerateDocumentFilterSet<T>()
        {
            var filters = (DocumentFilterAttribute[])typeof(T).GetTypeInfo().GetCustomAttributes(typeof (DocumentFilterAttribute), true);

            if (filters.Length == 0)
            {
                return null;
            }
            else 
            {
                return new DocumentFilterSet<T>(filters.Select(p => p.GetFilter<T>()));
            }
        }

    }
}
