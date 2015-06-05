using System;
using System.Collections.Generic;
using System.Linq;

namespace Couchbase.Linq.Filters
{
    /// <summary>
    /// Stores a list of <see cref="IEntityFilter">IEntityFilter</see>s, sorted by Priority
    /// </summary>
    /// <remarks>
    /// Sort order of IEntityFilters with the same Priority is undefined
    /// </remarks>
    class EntityFilterSet : SortedSet<IEntityFilter>
    {

        /// <summary>
        /// Create an empty EntityFilterSet
        /// </summary>
        public EntityFilterSet() : base(new PriorityComparer())
        {
        }

        /// <summary>
        /// Create an EntityFilterSet, filled with a set of filters
        /// </summary>
        public EntityFilterSet(IEnumerable<IEntityFilter> filters) : base(filters, new PriorityComparer())
        {   
        }

        /// <summary>
        /// Apply the filters to a LINQ query, in order
        /// </summary>
        public IQueryable<T> ApplyFilters<T>(IQueryable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            foreach (var filter in this)
            {
                source = filter.ApplyFilter(source);
            }

            return source;
        }

        private class PriorityComparer : IComparer<IEntityFilter>
        {

            public int Compare(IEntityFilter x, IEntityFilter y)
            {
                return x.Priority.CompareTo(y.Priority);
            }

        }
    }
}
