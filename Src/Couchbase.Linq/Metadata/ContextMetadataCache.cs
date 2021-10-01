using System;
using System.Collections.Concurrent;

namespace Couchbase.Linq.Metadata
{
    /// <summary>
    /// Static cache of <see cref="ContextMetadata"/> keyed by type inherited from <see cref="BucketContext"/>.
    /// </summary>
    internal class ContextMetadataCache
    {
        public static ContextMetadataCache Instance { get; } = new();

        private readonly ConcurrentDictionary<Type, ContextMetadata> _cache = new();

        public ContextMetadata Get(Type type)
        {
            return _cache.GetOrAdd(type, static t => new ContextMetadata(t));
        }
    }
}
