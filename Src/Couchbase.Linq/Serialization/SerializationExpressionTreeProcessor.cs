using System;
using System.Linq.Expressions;
using Couchbase.Core.Serialization;
using Remotion.Linq.Parsing.Structure;

namespace Couchbase.Linq.Serialization
{
    /// <summary>
    /// Process the tree to find members which have non-default serialization rules,
    /// and where present apply conversion expressions.  Internally uses
    /// <see cref="SerializationExpressionTreeVisitor"/>.
    /// </summary>
    internal class SerializationExpressionTreeProcessor : IExpressionTreeProcessor
    {
        private readonly ISerializationConverterProvider _serializationConverterProvider;

        public SerializationExpressionTreeProcessor(ISerializationConverterProvider serializationConverterProvider)
        {
            _serializationConverterProvider = serializationConverterProvider ??
                                              throw new ArgumentNullException(nameof(serializationConverterProvider));
        }

        /// <summary>
        /// Creates a <see cref="SerializationExpressionTreeProcessor"/> from a <see cref="IBucketContext"/>.
        /// </summary>
        /// <param name="bucketContext">The <see cref="IBucketContext"/>.</param>
        /// <returns>The <see cref="SerializationExpressionTreeProcessor"/>.</returns>
        public static SerializationExpressionTreeProcessor FromBucketContext(IBucketContext bucketContext)
        {
            var serializerProvider = bucketContext.Bucket as ITypeSerializerProvider;

            var serializer = serializerProvider?.Serializer ?? bucketContext.Configuration.Serializer.Invoke();

            // ReSharper disable once SuspiciousTypeConversion.Global
            return new SerializationExpressionTreeProcessor(
                serializer as ISerializationConverterProvider ??
                new DefaultSerializationConverterProvider(serializer));
        }

        /// <inheritdoc/>
        public Expression Process(Expression expressionTree)
        {
            if (expressionTree == null)
            {
                throw new ArgumentNullException(nameof(expressionTree));
            }

            return new SerializationExpressionTreeVisitor(_serializationConverterProvider).Visit(expressionTree);
        }
    }
}
