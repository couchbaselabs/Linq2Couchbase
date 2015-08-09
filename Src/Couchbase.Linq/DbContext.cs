using System;
using System.Linq;
using Couchbase.Configuration.Client;
using Couchbase.Core;

namespace Couchbase.Linq
{
    public class DbContext : IDbContext
    {
        private IBucket _bucket;
        protected BucketConfiguration _bucketConfig;

        public DbContext(ICluster cluster, string bucketName)
        {
            Cluster = cluster;
            Configuration = cluster.Configuration;
            _bucket = cluster.OpenBucket(bucketName);
            _bucketConfig = Configuration.BucketConfigs[bucketName];
        }

        public ICluster Cluster { get; protected set; }

        public ClientConfiguration Configuration { get; protected set; }


        public IQueryable<T> Query<T>()
        {
            return new BucketQueryable<T>(_bucket);
        }

        public string BucketName { get { return _bucket.Name; } }
    }
}

#region [ License information          ]

/* ************************************************************
 *
 *    @author Couchbase <info@couchbase.com>
 *    @copyright 2015 Couchbase, Inc.
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *
 * ************************************************************/

#endregion
