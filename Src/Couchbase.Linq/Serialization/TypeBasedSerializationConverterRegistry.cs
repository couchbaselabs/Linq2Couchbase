using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Couchbase.Linq.Serialization.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Couchbase.Linq.Serialization
{
    /// <summary>
    /// Registry of <see cref="JsonConverter"/> types and their corresponding <see cref="ISerializationConverter"/> implementations.
    /// </summary>
    public class TypeBasedSerializationConverterRegistry : IJsonNetSerializationConverterRegistry,
        IEnumerable<KeyValuePair<Type, Type>>
    {
        /// <summary>
        /// Global instance of <see cref="TypeBasedSerializationConverterRegistry"/> which is used
        /// by default by <see cref="DefaultSerializationConverterProvider"/>.
        /// Built-in converters are registered by default.
        /// </summary>
        public static TypeBasedSerializationConverterRegistry Global { get; } = CreateDefaultRegistry();

        private readonly Dictionary<Type, Type> _registry = new Dictionary<Type, Type>();

        /// <summary>
        /// Create a new registry loaded with all built in <see cref="ISerializationConverter"/> implementations.
        /// </summary>
        /// <returns></returns>
        public static TypeBasedSerializationConverterRegistry CreateDefaultRegistry() => new TypeBasedSerializationConverterRegistry
        {
            // TODO: Enabled UnixMillisecondsConverter once available https://issues.couchbase.com/browse/NCBC-2539
            // { typeof(UnixMillisecondsConverter), typeof(UnixMillisecondsSerializationConverter) },
            { typeof(StringEnumConverter), typeof(StringEnumSerializationConverter<>)}
        };

        /// <summary>
        /// Add an <see cref="ISerializationConverter"/> implementation.
        /// </summary>
        /// <param name="jsonConverterType">Type of the <see cref="JsonConverter"/>.</param>
        /// <param name="serializationConverterType">Type of the <see cref="ISerializationConverter"/> implementation.</param>
        public void Add(Type jsonConverterType, Type serializationConverterType)
        {
            if (jsonConverterType == null)
            {
                throw new ArgumentNullException(nameof(jsonConverterType));
            }
            if (serializationConverterType == null)
            {
                throw new ArgumentNullException(nameof(serializationConverterType));
            }

            _registry.Add(jsonConverterType, serializationConverterType);
        }

        /// <summary>
        /// Remove an <see cref="ISerializationConverter"/> implementation.
        /// </summary>
        /// <param name="jsonConverterType">Type of the <see cref="JsonConverter"/>.</param>
        public void Remove(Type jsonConverterType)
        {
            if (jsonConverterType == null)
            {
                throw new ArgumentNullException(nameof(jsonConverterType));
            }

            _registry.Remove(jsonConverterType);
        }

        /// <inheritdoc/>
        public ISerializationConverter CreateSerializationConverter(JsonConverter jsonConverter, MemberInfo member)
        {
            if (jsonConverter == null)
            {
                throw new ArgumentNullException(nameof(jsonConverter));
            }

            if (_registry.TryGetValue(jsonConverter.GetType(), out var serializationConverterType))
            {
                return CreateConverter(serializationConverterType, jsonConverter, member);
            }

            return null;
        }

        private ISerializationConverter CreateConverter(Type converterType, JsonConverter jsonConverter,
            MemberInfo member)
        {
            if (converterType.GetTypeInfo().IsGenericTypeDefinition)
            {
                var memberType = GetMemberType(member);
                if (memberType == null)
                {
                    throw new NotSupportedException(
                        "Generic SerializationConverter Applied To A Member Other Than Field Or Property");
                }

                if (memberType.GetTypeInfo().IsGenericType && memberType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    // Extract the inner type if the type is nullable
                    memberType = memberType.GenericTypeArguments[0];
                }

                converterType = converterType.MakeGenericType(memberType);
            }

            var constructor = converterType.GetConstructor(new[] {typeof(JsonConverter), typeof(MemberInfo)});
            if (constructor != null)
            {
                return (ISerializationConverter) constructor.Invoke(new object[] {jsonConverter, member});
            }

            return (ISerializationConverter) Activator.CreateInstance(converterType);
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<Type, Type>> GetEnumerator()
        {
            return _registry.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static Type GetMemberType(MemberInfo member)
        {
            switch (member)
            {
                case FieldInfo field:
                    return field.FieldType;

                case PropertyInfo property:
                    return property.PropertyType;

                default:
                    return null;
            }
        }
    }
}
