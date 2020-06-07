using System;
using Couchbase.Linq.Filters;

namespace Couchbase.Linq
{
    /// <summary>
    /// Options to control queries against an <see cref="ICollectionContext"/>.
    /// </summary>
    [Flags]
    public enum BucketQueryOptions
    {
        /// <summary>
        /// No special options, use default behavior.
        /// </summary>
        None = 0,

        /// <summary>
        /// Supress all registered filters in the <see cref="DocumentFilterManager"/>.
        /// </summary>
        SuppressFilters = 1
    }
}
