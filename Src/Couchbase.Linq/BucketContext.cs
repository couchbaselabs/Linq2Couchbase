using System;
using System.Linq;
using Couchbase.Linq.Filters;
using Couchbase.Linq.Metadata;
using Couchbase.Linq.Utils;

namespace Couchbase.Linq
{
    /// <summary>
    /// Provides a single point of entry to a Couchbase bucket which makes it easier to compose
    /// and execute queries and to group together changes which will be submitted back into the bucket.
    /// </summary>
    public class BucketContext : IBucketContext
    {
        private readonly DocumentFilterManager _documentFilterManager;

        /// <summary>
        /// Unit testing seam only, do not use!
        /// </summary>
#pragma warning disable 8618
        internal BucketContext()
#pragma warning restore 8618
        {
        }

        /// <summary>
        /// Creates a new BucketContext for a given Couchbase bucket.
        /// </summary>
        /// <param name="bucket">Bucket referenced by the new BucketContext.</param>
        public BucketContext(IBucket bucket)
        {
            Bucket = bucket ?? throw new ArgumentNullException(nameof(bucket));

            try
            {
                _documentFilterManager =
                    bucket.Cluster.ClusterServices.GetRequiredService<DocumentFilterManager>();
            }
            catch (InvalidOperationException)
            {
                throw new CouchbaseException(
                    $"{nameof(DocumentFilterManager)} has not been registered with the Couchbase Cluster. Be sure {nameof(LinqClusterOptionsExtensions.AddLinq)} is called on ${nameof(ClusterOptions)} during bootstrap.");
            }

            var myType = GetType();
            if (myType != typeof(BucketContext))
            {
                // If this isn't a base BucketContext, fill any properties added by the inherited class
                ContextMetadataCache.Instance.Get(myType).Fill(this);
            }
        }

        /// <inheritdoc />
        public IBucket Bucket { get; }

        /// <inheritdoc />
        public TimeSpan? QueryTimeout { get; set; }

        /// <inheritdoc />
        public IQueryable<T> Query<T>() =>
            Query<T>(BucketQueryOptions.None);

        /// <inheritdoc />
        public IQueryable<T> Query<T>(BucketQueryOptions options)
        {
            var (scope, collection) = CollectionMetadataCache.Instance.GetCollection<T>();

            return Query<T>(scope, collection, options);
        }

        internal IQueryable<T> Query<T>(string scope, string collection, BucketQueryOptions options = BucketQueryOptions.None)
        {
            IQueryable<T> query = new CollectionQueryable<T>(Bucket.Scope(scope).Collection(collection), QueryTimeout);

            if ((options & BucketQueryOptions.SuppressFilters) == BucketQueryOptions.None)
            {
                query = _documentFilterManager.ApplyFilters(query);
            }

            return query;
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
