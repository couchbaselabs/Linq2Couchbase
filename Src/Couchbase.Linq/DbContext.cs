using System;
using System.Linq;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Linq.Filters;

namespace Couchbase.Linq
{
    /// <summary>
    /// Provides a single point of entry to a Couchbase bucket which makes it easier to compose
    /// and execute queries and to group togather changes which will be submitted back into the bucket.
    /// </summary>
    public class DbContext : IDbContext
    {
        private readonly IBucket _bucket;
        protected BucketConfiguration BucketConfig;

        public DbContext(Cluster cluster, string bucketName)
            : this(cluster, bucketName, string.Empty)
        {
        }

        public DbContext(Cluster cluster, string bucketName, string password)
        {
            Cluster = cluster;
            Configuration = Cluster.Configuration;
            _bucket = Cluster.OpenBucket(bucketName, password);
        }

        /// <summary>
        /// Gets a reference to the <see cref="Cluster" /> that the <see cref="IDbContext" /> is using.
        /// </summary>
        /// <value>
        /// The cluster.
        /// </value>
        public ICluster Cluster { get; protected set; }

        /// <summary>
        /// Gets the configuration for the current <see cref="Cluster" />.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public ClientConfiguration Configuration { get; protected set; }

        /// <summary>
        /// Queries the current <see cref="IBucket" /> for entities of type <see cref="T" />. This is the target of
        /// the Linq query requires that the associated JSON document have a type property that is the same as <see cref="T" />.
        /// </summary>
        /// <typeparam name="T">An entity or POCO representing the object graph of a JSON document.</typeparam>
        /// <returns></returns>
        public IQueryable<T> Query<T>()
        {
            return DocumentFilterManager.ApplyFilters(new BucketQueryable<T>(_bucket));
        }

        /// <summary>
        /// Gets the name of the <see cref="IBucket"/>.
        /// </summary>
        /// <value>
        /// The name of the bucket.
        /// </value>
        public string BucketName
        {
            get { return _bucket.Name; }
        }
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
