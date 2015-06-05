using System;
using System.Linq;

namespace Couchbase.Linq.Filters
{
    /// <summary>
    /// Abstract base class for attribute-based <see cref="IEntityFilter">IEntityFilter</see> implementations
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class EntityFilterAttribute : Attribute, IEntityFilter
    {
        /// <summary>
        /// Priority of this filter compared to other filters against the same type.  Lower priorities execute first.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Apply the filter to a LINQ query
        /// </summary>
        public abstract IQueryable<T> ApplyFilter<T>(IQueryable<T> source);

    }
}
