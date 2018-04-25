using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Couchbase.Core.Serialization;
using Newtonsoft.Json.Serialization;

namespace Couchbase.Linq.Serialization
{
    /// <summary>
    /// Implementation of <see cref="IDateTimeSerializationFormatProvider"/> used if the Couchbase serializer
    /// doesn't implement the interface.  Checks to see if the <see cref="JsonConverter"/> for the property
    /// is <see cref="UnixMillisecondsConverter"/> to determine if the format should be ISO8601 or Unix milliseconds.
    /// Only works with <see cref="DefaultSerializer"/>, if using a custom serializer please implement
    /// <see cref="IDateTimeSerializationFormatProvider"/> directly on the serializer.
    /// </summary>
    internal class DefaultDateTimeSerializationFormatProvider : IDateTimeSerializationFormatProvider
    {
        // Uses a weak table to track a cache for each ITypeSerializer instance
        // Because it's a weak table, this won't cause memory leaks and will cleanup as GC collects the serializers
        private static readonly
            ConditionalWeakTable<ITypeSerializer, ConcurrentDictionary<MemberInfo, DateTimeSerializationFormat>> CacheSet =
                new ConditionalWeakTable<ITypeSerializer, ConcurrentDictionary<MemberInfo, DateTimeSerializationFormat>>();

        private readonly ITypeSerializer _serializer;
        private readonly ConcurrentDictionary<MemberInfo, DateTimeSerializationFormat> _cache;

        public DefaultDateTimeSerializationFormatProvider(ITypeSerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

            _cache = CacheSet.GetOrCreateValue(serializer);
        }

        public DateTimeSerializationFormat GetDateTimeSerializationFormat(MemberInfo member)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            if (!(_serializer is DefaultSerializer defaultSerializer))
            {
                // Default behavior
                return DateTimeSerializationFormat.Iso8601;
            }

            return _cache.GetOrAdd(member, p =>
            {
                if (defaultSerializer.SerializerSettings.ContractResolver.ResolveContract(member.DeclaringType) is JsonObjectContract contract)
                {
                    var property = contract.Properties.FirstOrDefault(
                        q => q.UnderlyingName == member.Name && !q.Ignored);

                    if (property?.Converter is UnixMillisecondsConverter)
                    {
                        return DateTimeSerializationFormat.UnixMilliseconds;
                    }
                }

                // Default behavior
                return DateTimeSerializationFormat.Iso8601;
            });
        }
    }
}
