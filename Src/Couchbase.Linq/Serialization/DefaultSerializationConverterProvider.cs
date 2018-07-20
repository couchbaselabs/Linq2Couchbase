using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Couchbase.Core.Serialization;
using Couchbase.Linq.Serialization.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Couchbase.Linq.Serialization
{
    /// <summary>
    /// Implementation of <see cref="ISerializationConverterProvider"/> used if the Couchbase serializer
    /// doesn't implement the interface.  Checks for <see cref="JsonConverter"/> on the property
    /// and uses a globally defined <see cref="Registry"/> to acquire an appropriate <see cref="ISerializationConverter"/>.
    /// Only works Json.Net serialization, if using a custom serializer please implement
    /// <see cref="ISerializationConverterProvider"/> directly on the serializer.
    /// </summary>
    public class DefaultSerializationConverterProvider : ISerializationConverterProvider
    {
        // Uses a weak table to track a cache for each ITypeSerializer/ISerializationConverterRegistry pair
        // Because it's a weak table, this won't cause memory leaks and will cleanup as GC collects the serializers
        private static readonly
            ConditionalWeakTable<ITypeSerializer, ConcurrentDictionary<MemberInfo, ISerializationConverter>> CacheSet =
                new ConditionalWeakTable<ITypeSerializer, ConcurrentDictionary<MemberInfo, ISerializationConverter>>();

        private static IJsonNetSerializationConverterRegistry _registry =
            TypeBasedSerializationConverterRegistry.Global;

        /// <summary>
        /// Registry of <see cref="ISerializationConverter"/> to use for a given <see cref="JsonConverter"/>.
        /// By default, uses <see cref="TypeBasedSerializationConverterRegistry.Global"/>.
        /// </summary>
        public static IJsonNetSerializationConverterRegistry Registry
        {
            get => _registry;
            set => _registry = value ?? throw new ArgumentNullException(nameof(value));
        }

        private readonly ITypeSerializer _serializer;
        private readonly ConcurrentDictionary<MemberInfo, ISerializationConverter> _cache;

        public DefaultSerializationConverterProvider(ITypeSerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

            _cache = CacheSet.GetOrCreateValue(serializer);
        }

        /// <inheritdoc/>
        public ISerializationConverter GetSerializationConverter(MemberInfo member)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            if (!(_serializer is DefaultSerializer defaultSerializer))
            {
                // Default behavior
                return null;
            }

            return _cache.GetOrAdd(member, p =>
            {
                if (defaultSerializer.SerializerSettings.ContractResolver.ResolveContract(member.DeclaringType) is JsonObjectContract contract)
                {
                    var property = contract.Properties.FirstOrDefault(
                        q => q.UnderlyingName == member.Name && !q.Ignored);

                    var jsonConverter = property?.Converter;
                    if (jsonConverter != null)
                    {
                        return Registry.CreateSerializationConverter(jsonConverter, p);
                    }
                }

                // Default behavior
                return null;
            });
        }
    }
}
