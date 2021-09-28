using System;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.Core.Version;
using Couchbase.Linq.Utils;
using Couchbase.Linq.Versioning;

namespace Couchbase.Linq.IntegrationTests
{
    public class N1QlTestBase
    {
        public async Task EnsureIndexExists(IBucket bucket, string indexName, params string[] fields)
        {
            var manager = bucket.Cluster.QueryIndexes;

            var indexes = await manager.GetAllIndexesAsync(bucket.Name);

            if (indexes.All(p => p.Name != indexName))
            {
                // We need to create the index

                await manager.CreateIndexAsync(bucket.Name, indexName, fields);
            }
        }

        public async Task EnsurePrimaryIndexExists(IBucket bucket)
        {
            var manager = bucket.Cluster.QueryIndexes;

            var indexes = await manager.GetAllIndexesAsync(bucket.Name);

            if (indexes.All(p => !p.IsPrimary))
            {
                // We need to create the index

                await manager.CreatePrimaryIndexAsync(bucket.Name);
            }
        }

        /// <summary>
        /// Switches between two different queries depending on support for collections on the current version of Couchbase.
        /// </summary>
        /// <typeparam name="TBase">Base type to query if collections are not supported.</typeparam>
        /// <typeparam name="TCollection">Type to query if collections are supported. Should be annotated with <see cref="CouchbaseCollectionAttribute"/>.</typeparam>
        protected async Task<IQueryable<TBase>> SwitchIfCollectionsSupportedAsync<TBase, TCollection>(IBucketContext bucketContext)
            where TCollection : TBase
        {
            var versionProvider = TestSetup.Cluster.ClusterServices.GetRequiredService<IClusterVersionProvider>();
            var clusterVersion = await versionProvider.GetVersionAsync() ?? FeatureVersions.DefaultVersion;
            if (clusterVersion.Version < new Version(7, 0, 0))
            {
                return bucketContext.Query<TBase>();
            }

            return (IQueryable<TBase>) bucketContext.Query<TCollection>();
        }
    }
}