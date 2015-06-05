using System;

namespace Couchbase.Linq.Filters
{
    /// <summary>
    /// Generates an <see cref="EntityFilterSet">EntityFilterSet</see> for a particular type.
    /// </summary>
    interface IEntityFilterSetGenerator
    {

        /// <summary>
        /// Generates an <see cref="EntityFilterSet">EntityFilterSet</see> for a particular type.
        /// </summary>
        /// <returns>Returns null if there are no filters.  This is to improve efficieny.</returns>
        EntityFilterSet GenerateEntityFilterSet(Type type);

    }
}
