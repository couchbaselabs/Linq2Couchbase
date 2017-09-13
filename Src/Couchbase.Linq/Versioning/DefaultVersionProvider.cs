using System;
using System.Collections.Concurrent;
using System.Threading;
using Couchbase.Core;
using Couchbase.Core.Version;
using Couchbase.Logging;

namespace Couchbase.Linq.Versioning
{
    /// <summary>
    /// Provides the version for a bucket based on the implementationVersion returned by calls
    /// to http://node-ip:8091/pools.  Caches the results for quick results on future calls.
    /// </summary>
    internal class DefaultVersionProvider : IVersionProvider
    {
        private readonly ILog _log = LogManager.GetLogger<DefaultVersionProvider>();

        private readonly ConcurrentDictionary<ICluster, ClusterVersion> _versionsByUri =
            new ConcurrentDictionary<ICluster, ClusterVersion>();
        private readonly object _lock = new object();

        /// <summary>
        /// Gets the version of the cluster hosting a bucket.
        /// </summary>
        /// <param name="bucket">Couchbase bucket.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bucket"/> is null.</exception>
        /// <returns>The version of the cluster hosting this bucket, or 4.0.0 if unable to determine the version.</returns>
        public ClusterVersion GetVersion(IBucket bucket)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }

            var cluster = bucket.Cluster;

            // First check for an existing result
            var version = CacheLookup(cluster);
            if (version != null)
            {
                return version.Value;
            }

            var contextCache = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);

            try
            {
                // Only check one cluster at a time, this prevents multiple lookups during bootstrap
                lock (_lock)
                {
                    // In case the version was received while we were waiting for the lock, check for it again
                    version = CacheLookup(cluster);
                    if (version != null)
                    {
                        return version.Value;
                    }

                    try
                    {
                        // Use the bucket to get the cluster version, in case we're using old-style bucket passwords
                        version = bucket.GetClusterVersionAsync().Result;

                        if (version != null)
                        {
                            CacheStore(cluster, version.Value);
                            return version.Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Unhandled error getting cluster version", ex);

                        // Don't cache on exception, but assume 4.0 for now
                        return new ClusterVersion(new Version(4, 0, 0));
                    }

                    // No version information could be loaded from any node
                    var fallbackVersion = new ClusterVersion(new Version(4, 0, 0));
                    CacheStore(cluster, fallbackVersion);
                    return fallbackVersion;
                }
            }
            finally
            {
                if (contextCache != null)
                {
                    SynchronizationContext.SetSynchronizationContext(contextCache);
                }
            }
        }

        internal virtual ClusterVersion? CacheLookup(ICluster cluster)
        {
            if (_versionsByUri.TryGetValue(cluster, out var version))
            {
                return version;
            }

            return null;
        }

        internal virtual void CacheStore(ICluster cluster, ClusterVersion version)
        {
            _versionsByUri.AddOrUpdate(cluster, version, (key, oldValue) => version);
        }
    }
}
