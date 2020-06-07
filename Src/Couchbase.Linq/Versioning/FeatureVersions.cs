using System;
using System.Collections.Generic;
using Couchbase.Core.Version;

namespace Couchbase.Linq.Versioning
{
    /// <summary>
    /// Constants for the Couchbase versions where new features are implemented.
    /// </summary>
    internal static class FeatureVersions
    {
        /// <summary>
        /// Default version we assume if we can't get the cluster version.
        /// </summary>
        public static readonly ClusterVersion DefaultVersion = new ClusterVersion(new Version(4, 0, 0));

        /// <summary>
        /// Version where support was added for index-based joins where the primary key for the join
        /// is on the left instead of the right.
        /// </summary>
        public static readonly ClusterVersion IndexJoin = new ClusterVersion(new Version(4, 5, 0));

        /// <summary>
        /// Version where support was added for full ANSI joins.
        /// </summary>
        public static readonly ClusterVersion AnsiJoin = new ClusterVersion(new Version(5, 5, 0));

        /// <summary>
        /// Version where support was added for RYOW consistency.
        /// </summary>
        public static readonly ClusterVersion ReadYourOwnWrite = new ClusterVersion(new Version(4, 5, 0));

        /// <summary>
        /// Version where support was added for array indexes.
        /// </summary>
        public static readonly ClusterVersion ArrayIndexes = new ClusterVersion(new Version(4, 5, 0));

        /// <summary>
        /// Version where support was added for SELECT RAW
        /// </summary>
        public static readonly ClusterVersion SelectRaw = new ClusterVersion(new Version(5, 0, 0));

        /// <summary>
        /// Version where support was added for SELECT ... FROM queries on arrays
        /// </summary>
        public static readonly ClusterVersion ArrayInFromClause = new ClusterVersion(new Version(5, 0, 0));
    }
}
