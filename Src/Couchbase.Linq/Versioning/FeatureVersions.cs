using System;
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
        public static readonly ClusterVersion DefaultVersion = new ClusterVersion(new Version(5, 5, 0));
    }
}
