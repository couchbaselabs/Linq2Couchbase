using System;

#nullable enable

namespace Couchbase.Linq
{
    /// <summary>
    /// <see cref="ClusterOptions"/> extensions.
    /// </summary>
    public static class LinqClusterOptionsExtensions
    {
        /// <summary>
        /// Add Linq2Couchbase support to the cluster.
        /// </summary>
        /// <param name="options">Options to extend.</param>
        /// <returns>The <see cref="ClusterOptions"/> to allow chaining.</returns>
        public static ClusterOptions AddLinq(this ClusterOptions options) =>
            options.AddLinq(null);

        /// <summary>
        /// Add Linq2Couchbase support to the cluster.
        /// </summary>
        /// <param name="options">Options to extend.</param>
        /// <param name="setupAction">Action to apply additional configuration to <see cref="CouchbaseLinqConfiguration"/>.</param>
        /// <returns>The <see cref="ClusterOptions"/> to allow chaining.</returns>
        public static ClusterOptions AddLinq(this ClusterOptions options,
            Action<CouchbaseLinqConfiguration>? setupAction)
        {
            var configuration = new CouchbaseLinqConfiguration();

            setupAction?.Invoke(configuration);

            return options
                .AddClusterService(configuration.DocumentFilterManager);
        }
    }
}
