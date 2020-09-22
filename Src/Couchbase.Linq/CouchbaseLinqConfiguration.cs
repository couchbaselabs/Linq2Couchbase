using System;
using Couchbase.Core.IO.Serializers;
using Couchbase.Linq.Filters;
using Couchbase.Linq.Serialization;
using Couchbase.Linq.Utils;
using Newtonsoft.Json;

#nullable enable

namespace Couchbase.Linq
{
    /// <summary>
    /// Configuration for Linq2Couchbase.
    /// </summary>
    /// <remarks>
    /// Can be configured during calls to <see cref="LinqClusterOptionsExtensions.AddLinq(ClusterOptions)"/>.
    /// </remarks>
    public class CouchbaseLinqConfiguration
    {
        public CouchbaseLinqConfiguration()
        {
            this.WithJsonNetConverterProvider();
        }

        /// <summary>
        /// Factory used to create an <see cref="ISerializationConverterProvider"/> when required.
        /// </summary>
        internal Func<IServiceProvider, ISerializationConverterProvider> SerializationConverterProviderFactory
        {
            get;
            private set;
        } = null!; // Will be set by constructor, override null safety

        /// <summary>
        /// A <see cref="DocumentFilterManager"/> which registers various extensions that control how
        /// POCOs are filtered from the bucket. By default, POCOs are inspected for <see cref="DocumentFilterAttribute"/>
        /// attributes, such as the <see cref="DocumentTypeFilterAttribute"/>.
        /// </summary>
        public DocumentFilterManager DocumentFilterManager { get; } = new DocumentFilterManager();

        /// <summary>
        /// Sets a custom <see cref="ISerializationConverterProvider"/> using a factory method.
        /// </summary>
        /// <param name="factory">Factory method to create the <see cref="ISerializationConverterProvider"/>.</param>
        /// <returns>The configuration for method chaining.</returns>
        public CouchbaseLinqConfiguration WithSerializationConverterProvider(
            Func<IServiceProvider, ISerializationConverterProvider> factory)
        {
            SerializationConverterProviderFactory = factory ?? throw new ArgumentNullException(nameof(factory));

            return this;
        }
    }
}
