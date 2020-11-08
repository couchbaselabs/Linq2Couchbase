using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Couchbase.Core.IO.Serializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Couchbase.Linq.Serialization
{
    /// <summary>
    /// Implementation of <see cref="ISerializationConverterProvider"/> used if the Couchbase serializer
    /// doesn't implement the interface.  Checks for <see cref="JsonConverter"/> on the property
    /// and uses the globally configured <see cref="CouchbaseLinqConfiguration.WithSerializationConverterProvider"/> to acquire an
    /// appropriate <see cref="ISerializationConverter"/>. Only works for Newtonsoft.Json serialization, if using a custom serializer
    /// please implement a custom <see cref="ISerializationConverterProvider"/>.
    /// </summary>
    internal class DefaultSerializationConverterProvider : ISerializationConverterProvider
    {
        // Uses a weak table to track a cache for each ITypeSerializer/ISerializationConverterRegistry pair
        // Because it's a weak table, this won't cause memory leaks and will cleanup as GC collects the serializers
        private static readonly
            ConditionalWeakTable<ITypeSerializer, ConcurrentDictionary<MemberInfo, ISerializationConverter>> CacheSet =
                new ConditionalWeakTable<ITypeSerializer, ConcurrentDictionary<MemberInfo, ISerializationConverter>>();

        private readonly IJsonNetSerializationConverterRegistry _converterRegistry;
        private readonly ITypeSerializer _serializer;
        private readonly ConcurrentDictionary<MemberInfo, ISerializationConverter> _cache;

        public DefaultSerializationConverterProvider(ITypeSerializer serializer, IJsonNetSerializationConverterRegistry converterRegistry)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _converterRegistry = converterRegistry ?? throw new ArgumentNullException(nameof(converterRegistry));

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
                    if (property != null)
                    {
                        var jsonConverter = GetJsonConverter(property, defaultSerializer);
                        if (jsonConverter != null)
                        {
                            return _converterRegistry.CreateSerializationConverter(jsonConverter, p);
                        }
                    }
                }

                // Default behavior
                return null;
            });
        }

        private static JsonConverter GetJsonConverter(JsonProperty property, DefaultSerializer defaultSerializer)
        {
            if (property.Converter != null)
            {
                return property.Converter;
            }

            var valueContract = defaultSerializer.SerializerSettings.ContractResolver
                .ResolveContract(property.PropertyType);
            if (valueContract?.Converter != null)
            {
                return valueContract.Converter;
            }

            var converters = defaultSerializer.SerializerSettings.Converters;
            return converters?.FirstOrDefault(p => p.CanConvert(property.PropertyType));
        }
    }
}
