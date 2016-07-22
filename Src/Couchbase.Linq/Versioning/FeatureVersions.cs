using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.Versioning
{
    /// <summary>
    /// Constants for the Couchbase versions where new features are implemented.
    /// </summary>
    internal static class FeatureVersions
    {
        /// <summary>
        /// Version where support was added for index-based joins where the primary key for the join
        /// is on the left instead of the right.
        /// </summary>
        public static readonly Version IndexJoin = new Version(4, 5, 0);

        /// <summary>
        /// Version where support was added for RYOW consistency.
        /// </summary>
        public static readonly Version ReadYourOwnWrite = new Version(4, 5, 0);

        /// <summary>
        /// Version where support was added for array indexes.
        /// </summary>
        public static readonly Version ArrayIndexes = new Version(4, 5, 0);
    }
}
