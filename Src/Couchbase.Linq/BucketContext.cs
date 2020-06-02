using System.Linq;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Linq.Filters;

namespace Couchbase.Linq
{
    /// <summary>
    /// Provides a single point of entry to a Couchbase bucket which makes it easier to compose
    /// and execute queries and to group together changes which will be submitted back into the bucket.
    /// </summary>
    public class BucketContext : IBucketContext
    {
        /// <summary>
        /// Creates a new BucketContext for a given Couchbase bucket.
        /// </summary>
        /// <param name="bucket">Bucket referenced by the new BucketContext.</param>
        public BucketContext(IBucket bucket)
        {
            Bucket = bucket;
        }

        /// <summary>
        /// Gets the bucket the <see cref="IBucketContext"/> was created against.
        /// </summary>
        /// <value>The <see cref="IBucket"/>.</value>
        public IBucket Bucket { get; private set; }

        /// <summary>
        /// Gets the configuration for the current <see cref="Cluster" />.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public ClientConfiguration Configuration
        {
            get { return Bucket.Configuration.PoolConfiguration.ClientConfiguration; }
        }

        /// <summary>
        /// Queries the current <see cref="IBucket" /> for entities of type T. This is the target of
        /// a LINQ query and requires that the associated JSON document have a type property that is the same as T.
        /// </summary>
        /// <typeparam name="T">An entity or POCO representing the object graph of a JSON document.</typeparam>
        /// <returns><see cref="IQueryable{T}" /> which can be used to query the bucket.</returns>
        public IQueryable<T> Query<T>()
        {
            return Query<T>(BucketQueryOptions.None);
        }

        /// <summary>
        /// Queries the current <see cref="IBucket" /> for entities of type T. This is the target of
        /// a LINQ query and requires that the associated JSON document have a type property that is the same as T.
        /// </summary>
        /// <typeparam name="T">An entity or POCO representing the object graph of a JSON document.</typeparam>
        /// <param name="options">Options to control the returned query.</param>
        /// <returns><see cref="IQueryable{T}" /> which can be used to query the bucket.</returns>
        public IQueryable<T> Query<T>(BucketQueryOptions options)
        {
            IQueryable<T> query = new BucketQueryable<T>(Bucket, Configuration, this);

            if ((options & BucketQueryOptions.SuppressFilters) == BucketQueryOptions.None)
            {
                query = DocumentFilterManager.ApplyFilters(query);
            }

            return query;
        }

        /// <summary>
        /// Gets the name of the <see cref="IBucket"/>.
        /// </summary>
        /// <value>
        /// The name of the bucket.
        /// </value>
        public string BucketName
        {
            get { return Bucket.Name; }
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
