using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core.Serialization;
using Couchbase.Core.Version;
using Couchbase.Linq.QueryGeneration.MemberNameResolvers;
using Couchbase.Linq.Serialization;
using Couchbase.Linq.Versioning;
using Newtonsoft.Json.Serialization;
using Remotion.Linq.Clauses.Expressions;

namespace Couchbase.Linq.QueryGeneration
{
    /// <summary>
    /// Used to pass query generation context between various classes
    /// </summary>
    internal class N1QlQueryGenerationContext
    {
        private IDateTimeSerializationFormatProvider _dateTimeSerializationFormatProvider;

        public N1QlExtentNameProvider ExtentNameProvider { get; set; }
        public IMemberNameResolver MemberNameResolver { get; set; }
        public IMethodCallTranslatorProvider MethodCallTranslatorProvider { get; set; }
        public ParameterAggregator ParameterAggregator { get; set; }
        public ITypeSerializer Serializer { get; set; }
        public ClusterVersion ClusterVersion { get; set; }

        /// <summary>
        /// Stores a reference to the current grouping subquery
        /// </summary>
        public QuerySourceReferenceExpression GroupingQuerySource { get; set; }

        /// <summary>
        /// If true, indicates that the document metadata should also be included in the select projection as "__metadata"
        /// </summary>
        public bool SelectDocumentMetadata { get; set; }

        /// <summary>
        /// <see cref="IDateTimeSerializationFormatProvider"/> to use when determining the serialization format
        /// of DateTime properties.
        /// </summary>
        public IDateTimeSerializationFormatProvider DateTimeSerializationFormatProvider
        {
            get
            {
                if (_dateTimeSerializationFormatProvider == null)
                {
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    _dateTimeSerializationFormatProvider = Serializer as IDateTimeSerializationFormatProvider ??
                                                           new DefaultDateTimeSerializationFormatProvider(Serializer);
                }

                return _dateTimeSerializationFormatProvider;
            }
        }

        public N1QlQueryGenerationContext()
        {
            ExtentNameProvider = new N1QlExtentNameProvider();
            ParameterAggregator = new ParameterAggregator();
        }

        /// <summary>
        /// Clones this N1QlQueryGenerationContext for use within a union secondary query
        /// </summary>
        public N1QlQueryGenerationContext CloneForUnion()
        {
            // In the future we may want some properties get new values when working in a union
            // This method provides a simple point for this extension

            return new N1QlQueryGenerationContext()
            {
                ExtentNameProvider = ExtentNameProvider,
                MemberNameResolver = MemberNameResolver,
                MethodCallTranslatorProvider = MethodCallTranslatorProvider,
                ParameterAggregator = ParameterAggregator,
                Serializer = Serializer,
                ClusterVersion = ClusterVersion
            };
        }
    }
}
