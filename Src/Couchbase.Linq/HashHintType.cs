using System;

namespace Couchbase.Linq
{
    /// <summary>
    /// Provides a hint when performing a hash join.
    /// </summary>
    public enum HashHintType
    {
        /// <summary>
        /// A lookup table should be built from the items on the right side of the join, and items on the left side
        /// should probe this lookup table to find matches. Typically used when the number of items on the right side
        /// of the join is smaller.
        /// </summary>
        Build = 0,

        /// <summary>
        /// A lookup table should be built from the items on the left side of the join, and items on the right side
        /// should probe this lookup table to find matches. Typically used when the number of items on the left side
        /// of the join is smaller.
        /// </summary>
        Probe = 1
    }
}
