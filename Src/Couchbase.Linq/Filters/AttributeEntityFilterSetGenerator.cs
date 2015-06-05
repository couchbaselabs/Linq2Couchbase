using System;
using System.Linq;

namespace Couchbase.Linq.Filters
{
    /// <summary>
    /// Generates an <see cref="EntityFilterSet">EntityFilterSet</see> for a particular type, using <see cref="EntityFilterAttribute">EntityFilterAttributes</see>
    /// </summary>
    class AttributeEntityFilterSetGenerator : IEntityFilterSetGenerator
    {

        /// <summary>
        /// Generates an <see cref="EntityFilterSet">EntityFilterSet</see> for a particular type, using <see cref="EntityFilterAttribute">EntityFilterAttribute</see>s
        /// </summary>
        /// <returns>Returns null if there are no filters.  This is to improve efficieny.</returns>
        public EntityFilterSet GenerateEntityFilterSet(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            var filters = type.GetCustomAttributes(typeof (EntityFilterAttribute), true);

            if (filters.Length == 0)
            {
                return null;
            }
            else 
            {
                return new EntityFilterSet(filters.Cast<IEntityFilter>());
            }
        }

    }
}
