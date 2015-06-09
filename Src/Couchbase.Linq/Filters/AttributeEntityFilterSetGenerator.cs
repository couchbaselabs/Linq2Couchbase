using System;
using System.Linq;

namespace Couchbase.Linq.Filters
{
    /// <summary>
    /// Generates an <see cref="EntityFilterSet&lt;T&gt;">EntityFilterSet</see> for a particular type, using <see cref="EntityFilterAttribute">EntityFilterAttributes</see>
    /// </summary>
    public class AttributeEntityFilterSetGenerator : IEntityFilterSetGenerator
    {

        /// <summary>
        /// Generates an <see cref="EntityFilterSet&lt;T&gt;">EntityFilterSet</see> for a particular type, using <see cref="EntityFilterAttribute">EntityFilterAttribute</see>s
        /// </summary>
        /// <returns>Returns null if there are no filters.  This is to improve efficieny.</returns>
        public EntityFilterSet<T> GenerateEntityFilterSet<T>()
        {
            var filters = (EntityFilterAttribute[])typeof(T).GetCustomAttributes(typeof (EntityFilterAttribute), true);

            if (filters.Length == 0)
            {
                return null;
            }
            else 
            {
                return new EntityFilterSet<T>(filters.Select(p => p.GetFilter<T>()));
            }
        }

    }
}
