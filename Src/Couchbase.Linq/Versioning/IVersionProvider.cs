using System;
using Couchbase.Core;
using Couchbase.Core.Version;

namespace Couchbase.Linq.Versioning
{
    /// <summary>
    /// Provides the Couchbase version for a bucket.
    /// </summary>
    internal interface IVersionProvider
    {
        /// <summary>
        /// Gets the version of the cluster hosting a bucket.
        /// </summary>
        /// <param name="bucket">Couchbase bucket.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bucket"/> is null.</exception>
        /// <returns>The version of the cluster hosting this bucket, or 4.0.0 if unable to determine the version.</returns>
        ClusterVersion GetVersion(IBucket bucket);
    }
}
