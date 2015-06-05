using System.Linq;

namespace Couchbase.Linq.Filters
{
    /// <summary>
    /// Filter designed to be applied to a LINQ query automatically
    /// </summary>
    interface IEntityFilter
    {
        /// <summary>
        /// Priority of this filter compared to other filters against the same type.  Lower priorities execute first.
        /// </summary>
        int Priority { get; set; }

        /// <summary>
        /// Apply the filter to a LINQ query
        /// </summary>
        IQueryable<T> ApplyFilter<T>(IQueryable<T> source);
    }
}
