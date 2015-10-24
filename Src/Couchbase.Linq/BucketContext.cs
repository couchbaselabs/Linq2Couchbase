using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Linq.Filters;
using Couchbase.Linq.Utils;

namespace Couchbase.Linq
{
    /// <summary>
    /// Provides a single point of entry to a Couchbase bucket which makes it easier to compose
    /// and execute queries and to group togather changes which will be submitted back into the bucket.
    /// </summary>
    public class BucketContext : IBucketContext
    {
        private readonly IBucket _bucket;
        protected BucketConfiguration BucketConfig;
        private Dictionary<Type, PropertyInfo>_cachedKeyProperties = new Dictionary<Type, PropertyInfo>();

        /// <summary>
        /// If true, generate change tracking proxies for documents during deserialization.  Defaults to false for higher performance queries.
        /// </summary>
        public virtual bool EnableChangeTracking { get; set; }

        public BucketContext(IBucket bucket)
        {
            _bucket = bucket;
        }

        /// <summary>
        /// Gets the configuration for the current <see cref="Cluster" />.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public ClientConfiguration Configuration
        {
            get { return _bucket.Configuration.PoolConfiguration.ClientConfiguration; }
        }

        /// <summary>
        /// Queries the current <see cref="IBucket" /> for entities of type <see cref="T" />. This is the target of
        /// the Linq query requires that the associated JSON document have a type property that is the same as <see cref="T" />.
        /// </summary>
        /// <typeparam name="T">An entity or POCO representing the object graph of a JSON document.</typeparam>
        /// <returns></returns>
        public IQueryable<T> Query<T>()
        {
            return DocumentFilterManager.ApplyFilters(new BucketQueryable<T>(_bucket, Configuration, EnableChangeTracking));
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

        /// <summary>
        /// Saves the specified document.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document">The document.</param>
        /// <exception cref="KeyAttributeMissingException">The document id could not be found.</exception>
        /// <exception cref="AmbiguousMatchException">More than one of the requested attributes was found.</exception>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        /// <exception cref="CouchbaseWriteException">An exception wrapping the <see cref="IOperationResult"/> interface. Use this to determine what failed.</exception>
        public void Save<T>(T document)
        {
            var id = GetDocumentId(document);
            var result = _bucket.Upsert(id, document);
            if (!result.Success)
            {
                throw new CouchbaseWriteException(result);
            }
        }

        /// <exception cref="KeyAttributeMissingException">The document id could not be found.</exception>
        /// <exception cref="AmbiguousMatchException">More than one of the requested attributes was found. </exception>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded. </exception>
        /// <exception cref="CouchbaseWriteException">An exception wrapping the <see cref="IOperationResult"/> interface. Use this to determine what failed.</exception>
        public void Remove<T>(T document)
        {
            var id = GetDocumentId(document);
            var result = _bucket.Remove(id);
            if (!result.Success)
            {
                throw new CouchbaseWriteException(result);
            }
        }

        /// <summary>
        /// Gets the document identifier. Assumes that at least one property on the document has a
        /// <see cref="KeyAttribute"/> which defines the unique indentifier field for the document.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        /// <exception cref="KeyAttributeMissingException">The document document key could not be found.</exception>
        /// <exception cref="AmbiguousMatchException">More than one of the requested attributes was found.</exception>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        internal string GetDocumentId<T>(T document)
        {
            var type = document.GetType();

            PropertyInfo propertyInfo;
            if (_cachedKeyProperties.TryGetValue(type, out propertyInfo))
            {
                return (string) propertyInfo.GetValue(document);
            }
            var properties = type.GetProperties();
            foreach (var pi in properties)
            {
                var attribute = (KeyAttribute)Attribute.
                    GetCustomAttribute(pi, typeof(KeyAttribute));

                if (attribute != null)
                {
                    _cachedKeyProperties.Add(type, pi);
                    return (string)pi.GetValue(document);
                }
            }
            throw new KeyAttributeMissingException(ExceptionMsgs.KeyAttributeMissing);
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
