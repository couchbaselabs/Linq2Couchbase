using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Couchbase.Linq.Filters
{
    /// <summary>
    /// Caches reflection information regarding <see cref="CouchbaseCollectionAttribute"/>.
    /// </summary>
    internal class CollectionMetadataCache
    {
        public static CollectionMetadataCache Instance { get; } = new();

        private readonly ConcurrentDictionary<Type, CouchbaseCollectionAttribute> _typeCache = new();

        public CouchbaseCollectionAttribute GetCollection<T>() =>
            GetCollection(typeof(T));

        public CouchbaseCollectionAttribute GetCollection(Type type) =>
            _typeCache.GetOrAdd(type, static key => key.GetCustomAttribute<CouchbaseCollectionAttribute>()
                                                    ?? CouchbaseCollectionAttribute.Default);
    }
}
