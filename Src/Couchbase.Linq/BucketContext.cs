using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Linq.Filters;
using Couchbase.Linq.Metadata;
using Couchbase.Linq.Proxies;
using Couchbase.Linq.Utils;
using Couchbase.N1QL;

namespace Couchbase.Linq
{
    /// <summary>
    /// Provides a single point of entry to a Couchbase bucket which makes it easier to compose
    /// and execute queries and to group togather changes which will be submitted back into the bucket.
    /// </summary>
    public class BucketContext : IBucketContext, IChangeTrackableContext
    {
        private readonly IBucket _bucket;
        private readonly ConcurrentDictionary<Type, PropertyInfo>_cachedKeyProperties = new ConcurrentDictionary<Type, PropertyInfo>();
        private readonly ConcurrentDictionary<string, object> _tracked = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, object> _modified = new ConcurrentDictionary<string, object>();
        private int _beginChangeTrackingCount = 0;

        /// <summary>
        /// If true, generate change tracking proxies for documents during deserialization.  Defaults to false for higher performance queries.
        /// </summary>
        public bool ChangeTrackingEnabled { get { return _beginChangeTrackingCount > 0; } }

        /// <summary>
        /// Execute a N1QL query
        /// </summary>
        /// <param name="statement">The N1QL statement</param>
        /// <param name="parameters">Positional parameters to replace tokens of the form "$1", "$2", etc</param>
        public void Execute(string statement, params object[] parameters)
        {
            var query = new QueryRequest(statement)
                .AddPositionalParameter(parameters);
            _bucket.Query<dynamic>(query);
        }

        /// <summary>
        /// Access to the underlying Bucket to drop down to lower-level API when necessary.
        /// </summary>

        /// <summary>
        /// Creates a new BucketContext for a given Couchbase bucket.
        /// </summary>
        /// <param name="bucket">Bucket referenced by the new BucketContext.</param>
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
        /// Queries the current <see cref="IBucket" /> for entities of type T. This is the target of
        /// a LINQ query and requires that the associated JSON document have a type property that is the same as T.
        /// </summary>
        /// <typeparam name="T">An entity or POCO representing the object graph of a JSON document.</typeparam>
        /// <returns><see cref="IQueryable{T}" /> which can be used to query the bucket.</returns>
        public IQueryable<T> Query<T>()
        {
            return DocumentFilterManager.ApplyFilters(new BucketQueryable<T>(_bucket, Configuration, this));
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
        /// Saves the specified document to a Couchbase server cluster. If change tracking is enabled via <see cref="BeginChangeTracking"/>
        /// then the document will be added to the modified list and then saved when <see cref="SubmitChanges"/> is called.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the document being saved.</typeparam>
        /// <param name="document">The document.</param>
        /// <exception cref="KeyAttributeMissingException">The document id could not be found.</exception>
        /// <exception cref="AmbiguousMatchException">More than one of the requested attributes was found.</exception>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        /// <exception cref="CouchbaseWriteException">An exception wrapping the <see cref="IOperationResult"/> interface. Use this to determine what failed.</exception>
        public void Save<T>(T document)
        {
            var id = GetDocumentId(document);
            if (ChangeTrackingEnabled)
            {
                var documentWrapper = new NewDocumentWrapper
                {
                    Value = document,
                    Metadata = new DocumentMetadata {Id = id},
                    IsDirty = true
                };
                documentWrapper.RegisterChangeTracking(this); //overkill with no effect

                var context = this as IChangeTrackableContext;
                context.Track(documentWrapper);
                context.Modified(documentWrapper);
            }
            else
            {
                var result = _bucket.Upsert(id, document);
                if (!result.Success)
                {
                    throw new CouchbaseWriteException(result);
                }
            }
        }

        /// Removes a document from a Couchbase server cluster. If change tracking is enabled, the document will be flagged for deletion
        /// and deleted when <see cref="SubmitChanges"/> is called on the current <see cref="BucketContext"/>.
        /// <exception cref="KeyAttributeMissingException">The document id could not be found.</exception>
        /// <exception cref="AmbiguousMatchException">More than one of the requested attributes was found. </exception>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded. </exception>
        /// <exception cref="CouchbaseWriteException">An exception wrapping the <see cref="IOperationResult"/> interface. Use this to determine what failed.</exception>
        public void Remove<T>(T document)
        {
            if (ChangeTrackingEnabled)
            {
                var trackedDocument = (ITrackedDocumentNode) document;
                trackedDocument.IsDeleted = true;

                var context = this as IChangeTrackableContext;
                context.Modified(document);
            }
            else
            {
                var id = GetDocumentId(document);
                var result = _bucket.Remove(id);
                if (!result.Success)
                {
                    throw new CouchbaseWriteException(result);
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
        /// <exception cref="KeyAttributeMissingException">The document document key could not be found.</exception>
        /// <exception cref="AmbiguousMatchException">More than one of the requested attributes was found.</exception>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        internal string GetDocumentId<T>(T document)
        {
            var type = document.GetType();

            PropertyInfo propertyInfo;
            if (_cachedKeyProperties.TryGetValue(type, out propertyInfo))
            {
                if (propertyInfo.Name == "Metadata")
                {
                    return (string)propertyInfo.GetValue(((ITrackedDocumentNode)document).Metadata);
                }
                if (document is ITrackedDocumentNode)
                {
                    return (string) propertyInfo.GetValue(((ITrackedDocumentNode) document).Metadata);
                }
            }
            var properties = type.GetProperties();
            foreach (var pi in properties)
            {
                var attribute = (KeyAttribute)Attribute.
                    GetCustomAttribute(pi, typeof(KeyAttribute));

                if (attribute != null || pi.Name == "Metadata")
                {
                    if (pi.Name == "Metadata")
                    {
                        var metadataPi = pi.PropertyType.GetProperty("Id");
                        _cachedKeyProperties.TryAdd(type, metadataPi);
                        return (string)metadataPi.GetValue(((ITrackedDocumentNode)document).Metadata);
                    }
                    _cachedKeyProperties.TryAdd(type, pi);
                    return (string)pi.GetValue(document);
                }
            }
            throw new KeyAttributeMissingException(ExceptionMsgs.KeyAttributeMissing);
        }

        /// <summary>
        /// Begins change tracking for the current request. To complete and save the changes call <see cref="SubmitChanges" />.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void BeginChangeTracking()
        {
            Interlocked.Increment(ref _beginChangeTrackingCount);
        }

        /// <summary>
        /// Ends change tracking on the current <see cref="BucketContext"/> removing any changed docments from the tracked list.
        /// </summary>
        public void EndChangeTracking()
        {
            Interlocked.Decrement(ref _beginChangeTrackingCount);

            //release any tracked documents from change tracking
            lock (_tracked)
            {
                foreach (var node in _tracked)
                {
                    var tracked = node.Value as ITrackedDocumentNode;
                    var callback = tracked as ITrackedDocumentNodeCallback;
                    if (tracked != null) tracked.UnregisterChangeTracking(callback);
                }
                _tracked.Clear();
                _modified.Clear();
            }
        }

        /// <summary>
        /// Submits any changes to documents that have been modified if change tracking is enabled via a call to <see cref="BeginChangeTracking"/>.
        /// Internally a counter is kept so that if n threads call <see cref="BeginChangeTracking"/>, then n threads must call <see cref="SubmitChanges"/>.
        /// After submit changes is called, the modified list will be cleared.
        /// </summary>
        public void SubmitChanges()
        {
            Interlocked.Decrement(ref _beginChangeTrackingCount);
            if(_beginChangeTrackingCount < 1)
            {
                try
                {
                    foreach (var modified in _modified)
                    {
                        var doc = modified.Value as ITrackedDocumentNode;
                        if (doc != null && doc.IsDeleted)
                        {
                            var result = _bucket.Remove(modified.Key);
                            if (!result.Success)
                            {
                                throw new CouchbaseWriteException(result);
                            }
                        }
                        else if (doc != null && doc.IsDirty)
                        {
                            IOperationResult result = null;
                            if (doc is NewDocumentWrapper)
                            {
                                var newDocument = (doc as NewDocumentWrapper).Value;
                                result = _bucket.Upsert(modified.Key, newDocument);
                            }
                            else
                            {
                                result = _bucket.Upsert(modified.Key, modified.Value);
                            }
                            if (result != null && !result.Success)
                            {
                                throw new CouchbaseWriteException(result);
                            }
                        }
                    }
                }
                finally
                {
                    _modified.Clear();
                }
            }
        }

        /// <summary>
        /// Adds a document to the list of tracked documents if change tracking is enabled.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the document to track.</typeparam>
        /// <param name="document">The object representing the document.</param>
        void IChangeTrackableContext.Track<T>(T document)
        {
            if (ChangeTrackingEnabled)
            {
                var id = GetDocumentId(document);

                _tracked.AddOrUpdate(id, document, (k, v) => document);
            }
        }

        /// <summary>
        /// Removes a document from the list if tracked documents.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document"></param>
        void IChangeTrackableContext.Untrack<T>(T document)
        {
            if (ChangeTrackingEnabled)
            {
                var id = GetDocumentId(document);

                object temp;
                if (_tracked.TryRemove(id, out temp))
                {
                   //add logging
                }
            }
        }

        /// <summary>
        /// Adds a document to the list of modified documents if it is has been mutated and is being tracked.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document"></param>
        void IChangeTrackableContext.Modified<T>(T document)
        {
            if (ChangeTrackingEnabled)
            {
                var id = GetDocumentId(document);

                _modified.AddOrUpdate(id, document, (k, v) => document);
            }
        }

        /// <summary>
        /// The count of documents that have been modified if change tracking is enabled.
        /// </summary>
        public int ModifiedCount { get { return _modified.Count; } }

        /// <summary>
        /// The count of all documents currently being tracked.
        /// </summary>
        public int TrackedCount { get { return _tracked.Count; } }

        /// <summary>
        /// Handles documents that have been mutated when change tracking is enabled and adds them to the modified list.
        /// </summary>
        /// <param name="mutatedDocument"></param>
        void ITrackedDocumentNodeCallback.DocumentModified(ITrackedDocumentNode mutatedDocument)
        {
            object document;
            if(_tracked.TryGetValue(((DocumentNode)mutatedDocument).Metadata.Id, out document))
            {
                _modified.AddOrUpdate(((DocumentNode)mutatedDocument).Metadata.Id, document, (k, v) => document);
            }
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
