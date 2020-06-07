using System.Linq;
using System.Threading.Tasks;

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
    }
}