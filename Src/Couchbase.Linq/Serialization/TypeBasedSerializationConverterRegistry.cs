using System;
using System.Collections;
using System.Collections.Generic;
using Couchbase.Core.Serialization;
using Couchbase.Linq.Serialization.Converters;
using Newtonsoft.Json;

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
            { typeof(UnixMillisecondsConverter), typeof(UnixMillisecondsSerializationConverter) }
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
        public ISerializationConverter GetSerializationConverter(JsonConverter jsonConverter)
        {
            if (jsonConverter == null)
            {
                throw new ArgumentNullException(nameof(jsonConverter));
            }

            if (_registry.TryGetValue(jsonConverter.GetType(), out var serializationConverterType))
            {
                return (ISerializationConverter) Activator.CreateInstance(serializationConverterType);
            }

            return null;
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
    }
}
