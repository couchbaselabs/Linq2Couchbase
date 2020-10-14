using System;
using Couchbase.Core.IO.Serializers;
using Couchbase.Linq.Serialization;
using Couchbase.Linq.Utils;

#nullable enable

namespace Couchbase.Linq
{
    public static class CouchbaseLinqConfigurationExtensions
    {
        /// <summary>
        /// Sets the default <see cref="ISerializationConverterProvider"/> with a default <see cref="IJsonNetSerializationConverterRegistry"/>.
        /// </summary>
        /// <param name="configuration">The configuration to update.</param>
        /// <returns>The configuration for method chaining.</returns>
        public static CouchbaseLinqConfiguration WithJsonNetConverterProvider(
            this CouchbaseLinqConfiguration configuration) =>
            configuration.WithJsonNetConverterProvider((Action<TypeBasedSerializationConverterRegistry>?) null);

        /// <summary>
        /// Sets the default <see cref="ISerializationConverterProvider"/>.
        /// </summary>
        /// <param name="configuration">The configuration to update.</param>
        /// <param name="registryConfigurationAction">Action to configure the default registry.</param>
        /// <returns>The configuration for method chaining.</returns>
        public static CouchbaseLinqConfiguration WithJsonNetConverterProvider(
            this CouchbaseLinqConfiguration configuration,
            Action<TypeBasedSerializationConverterRegistry>? registryConfigurationAction)
        {
            return configuration.WithSerializationConverterProvider(serviceProvider =>
            {
                var registry = TypeBasedSerializationConverterRegistry.CreateDefaultRegistry();

                registryConfigurationAction?.Invoke(registry);

                return new DefaultSerializationConverterProvider(
                    serviceProvider.GetRequiredService<ITypeSerializer>(),
                    registry);
            });
        }

        /// <summary>
        /// Sets the default <see cref="ISerializationConverterProvider"/> with a custom registry.
        /// </summary>
        /// <param name="configuration">The configuration to update.</param>
        /// <param name="registry">The registry to use.</param>
        /// <returns>The configuration for method chaining.</returns>
        public static CouchbaseLinqConfiguration WithJsonNetConverterProvider(
            this CouchbaseLinqConfiguration configuration, IJsonNetSerializationConverterRegistry registry) =>
            configuration.WithSerializationConverterProvider(
                serviceProvider => new DefaultSerializationConverterProvider(
                    serviceProvider.GetRequiredService<ITypeSerializer>(),
                    registry ?? throw new ArgumentNullException(nameof(registry))));

        /// <summary>
        /// Sets a custom <see cref="ISerializationConverterProvider"/>.
        /// </summary>
        /// <param name="configuration">The configuration to update.</param>
        /// <param name="serializationConverterProvider">The serialization converter provider to use.</param>
        /// <returns>The configuration for method chaining.</returns>
        public static CouchbaseLinqConfiguration WithSerializationConverterProvider(
            this CouchbaseLinqConfiguration configuration, ISerializationConverterProvider serializationConverterProvider)
        {
            if (serializationConverterProvider == null)
            {
                throw new ArgumentNullException(nameof(serializationConverterProvider));
            }

            return configuration.WithSerializationConverterProvider(_ => serializationConverterProvider);
        }
    }
}
