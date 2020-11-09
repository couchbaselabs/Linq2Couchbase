using System;
using System.Linq.Expressions;
using Couchbase.Linq.Utils;
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
        /// Creates a <see cref="SerializationExpressionTreeProcessor"/> from a <see cref="ICluster"/>.
        /// </summary>
        /// <param name="cluster">The <see cref="ICluster"/>.</param>
        /// <returns>The <see cref="SerializationExpressionTreeProcessor"/>.</returns>
        public static SerializationExpressionTreeProcessor FromCluster(ICluster cluster) =>
            new SerializationExpressionTreeProcessor(
                cluster.ClusterServices.GetRequiredService<ISerializationConverterProvider>());

        /// <inheritdoc/>
        public Expression Process(Expression expressionTree)
        {
            if (expressionTree == null)
            {
                throw new ArgumentNullException(nameof(expressionTree));
            }

            return new SerializationExpressionTreeVisitor(_serializationConverterProvider).Visit(expressionTree)!;
        }
    }
}
