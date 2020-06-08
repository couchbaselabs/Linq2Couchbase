using System.Linq;
using Couchbase.KeyValue;
using Couchbase.Linq.Filters;

namespace Couchbase.Linq
{
    /// <summary>
    /// Provides a single point of entry to a Couchbase bucket which makes it easier to compose
    /// and execute queries and to group together changes which will be submitted back into the bucket.
    /// </summary>
    public class CollectionContext : ICollectionContext
    {
        /// <summary>
        /// Creates a new CollectionContext for a given Couchbase collection.
        /// </summary>
        /// <param name="collection">Collection referenced by the new CollectionContext.</param>
        public CollectionContext(ICouchbaseCollection collection)
        {
            Collection = collection;
        }

        /// <summary>
        /// Gets the collection the <see cref="ICollectionContext"/> was created against.
        /// </summary>
        /// <value>The <see cref="IBucket"/>.</value>
        public ICouchbaseCollection Collection { get; private set; }

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
            IQueryable<T> query = new CollectionQueryable<T>(Collection);

            if ((options & BucketQueryOptions.SuppressFilters) == BucketQueryOptions.None)
            {
                query = DocumentFilterManager.ApplyFilters(query);
            }

            return query;
        }

        /// <inheritdoc />
        public string CollectionName => Collection.Name;

        /// <inheritdoc />
        public string ScopeName => Collection.Scope.Name;

        /// <inheritdoc />
        public string BucketName => Collection.Scope.Bucket.Name;
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
