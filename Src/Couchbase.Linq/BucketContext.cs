using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.IO;
using Couchbase.Linq.Filters;
using Couchbase.Linq.Utils;
using Newtonsoft.Json;

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
            return DocumentFilterManager.ApplyFilters(new BucketQueryable<T>(_bucket, Configuration));
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
        /// <exception cref="DocumentIdMissingException">The document id could not be found.</exception>
        /// <exception cref="AmbiguousMatchException">More than one of the requested attributes was found.</exception>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        /// <exception cref="Exception">An internal exception was thrown.</exception>
        public void Save<T>(T document)
        {
            var id = GetDocumentId(document);
            var result = _bucket.Upsert(id, document);
            if (!result.Success)
            {
                if (result.Exception != null)
                {
                    // ReSharper disable once ThrowingSystemException
                    throw result.Exception;
                }
            }
        }

        /// <exception cref="DocumentIdMissingException">The document id could not be found.</exception>
        /// <exception cref="AmbiguousMatchException">More than one of the requested attributes was found. </exception>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded. </exception>
        /// <exception cref="DocumentNotFoundException">No document Id was found.</exception>
        /// <exception cref="Exception">An internal exception was thrown.</exception>
        public void Remove<T>(T document)
        {
            var id = GetDocumentId(document);
            var result = _bucket.Remove(id);
            if (!result.Success)
            {
                if (result.Status == ResponseStatus.KeyNotFound)
                {
                    // ReSharper disable once HeapView.ObjectAllocation
                    throw new DocumentNotFoundException(string.Format("{0}{1}", ExceptionMsgs.DocumentNotFound, id));
                }
                if (result.Exception != null)
                {
                    // ReSharper disable once ThrowingSystemException
                    throw result.Exception;
                }
            }
        }

        /// <summary>
        /// Gets the document identifier. Assumes that at least one property on the document has a
        /// <see cref="KeyAttribute"/> which defines the unique indentifier field for the document.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        /// <exception cref="DocumentIdMissingException">The document document id could not be found.</exception>
        /// <exception cref="AmbiguousMatchException">More than one of the requested attributes was found.</exception>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        internal string GetDocumentId<T>(T document)
        {
            var idName = string.Empty;
            var type = typeof(T);

            var properties = type.GetProperties();
            foreach (var propertyInfo in properties)
            {
                var attribute = (KeyAttribute)Attribute.
                    GetCustomAttribute(propertyInfo, typeof(KeyAttribute));

                if (attribute != null)
                {
                    var jsonPropertyAttribute = (JsonPropertyAttribute)Attribute.
                        GetCustomAttribute(propertyInfo, typeof(JsonPropertyAttribute));

                    if (jsonPropertyAttribute != null)
                    {
                        idName = jsonPropertyAttribute.PropertyName;
                        break;
                    }
                    idName = propertyInfo.Name;
                    break;
                }
            }
            if (string.IsNullOrWhiteSpace(idName))
            {
                throw new DocumentIdMissingException(ExceptionMsgs.DocumentIdMissing);
            }
            return idName;
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
