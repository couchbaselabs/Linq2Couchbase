using System;
using System.Configuration;
using System.Linq;
using Couchbase.Core;
using Couchbase.Management;
using NUnit.Framework;

namespace Couchbase.Linq.IntegrationTests
{
    public class N1QlTestBase
    {
        public IBucketManager GetBucketManager(IBucket bucket)
        {
            return bucket.CreateManager(ConfigurationManager.AppSettings["adminusername"],
                ConfigurationManager.AppSettings["adminpassword"]);
        }

        public void EnsureIndexExists(IBucket bucket, string indexName, params string[] fields)
        {
            var manager = GetBucketManager(bucket);

            var indexes = manager.ListN1qlIndexes();
            Assert.True(indexes.Success);

            if (indexes.All(p => p.Name != indexName))
            {
                // We need to create the index

                var result = manager.CreateN1qlIndex(indexName, false, fields);
                Assert.True(result.Success);
            }
        }

        public void EnsurePrimaryIndexExists(IBucket bucket)
        {
            var manager = GetBucketManager(bucket);

            var indexes = manager.ListN1qlIndexes();
            Assert.True(indexes.Success);

            if (indexes.All(p => !p.IsPrimary))
            {
                // We need to create the index

                var result = manager.CreateN1qlPrimaryIndex(false);
                Assert.True(result.Success);
            }
        }
    }
}