using System.Collections.Generic;
using Couchbase.Core.Serialization;
using Couchbase.Linq.Clauses;
using Couchbase.Linq.Operators;
using Couchbase.Linq.QueryGeneration.ExpressionTransformers;
using Couchbase.Linq.Serialization;
using Couchbase.Linq.Utils;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

namespace Couchbase.Linq
{
    internal class QueryParserHelper
    {
        private static readonly INodeTypeProvider _nodeTypeProvider;
        private static readonly ExpressionTransformerRegistry _transformerRegistry;

        static QueryParserHelper()
        {
            _nodeTypeProvider = CreateDefaultNodeTypeProvider();
            _transformerRegistry = CreateDefaultTransformerRegistry();
        }

        private static INodeTypeProvider CreateDefaultNodeTypeProvider()
        {
            //Create Custom node registry
            var nodeTypeRegistry = new MethodInfoBasedNodeTypeRegistry();

            //register the "Nest" clause type
            nodeTypeRegistry.Register(NestExpressionNode.SupportedMethods,
                typeof(NestExpressionNode));

            //register the "Explain" expression node parser
            nodeTypeRegistry.Register(ExplainExpressionNode.SupportedMethods,
                typeof(ExplainExpressionNode));

            //register the "UseKeys" expression node parser
            nodeTypeRegistry.Register(UseKeysExpressionNode.SupportedMethods,
                typeof(UseKeysExpressionNode));

            //register the "UseIndex" expression node parser
            nodeTypeRegistry.Register(UseIndexExpressionNode.SupportedMethods,
                typeof(UseIndexExpressionNode));

            //register the "UseHash" expression node parser
            nodeTypeRegistry.Register(UseHashExpressionNode.SupportedMethods,
                typeof(UseHashExpressionNode));

            //register the "ExtentName" expression node parser
            nodeTypeRegistry.Register(ExtentNameExpressionNode.SupportedMethods,
                typeof(ExtentNameExpressionNode));

            //register the "ToQueryRequest" expression node parser
            nodeTypeRegistry.Register(ToQueryRequestExpressionNode.SupportedMethods,
                typeof(ToQueryRequestExpressionNode));

            //This creates all the default node types
            var nodeTypeProvider = ExpressionTreeParser.CreateDefaultNodeTypeProvider();

            //add custom node provider to the providers
            nodeTypeProvider.InnerProviders.Add(nodeTypeRegistry);

            return nodeTypeProvider;
        }

        private static ExpressionTransformerRegistry CreateDefaultTransformerRegistry()
        {
            var transformerRegistry = ExpressionTransformerRegistry.CreateDefault();

            //Register transformer to handle enum == and != comparisons
            transformerRegistry.Register(new EnumComparisonExpressionTransformer());

            //Register transformer to handle string comparisons
            transformerRegistry.Register(new StringComparisonExpressionTransformer());

            //Register transformer to handle DateTime comparisons
            transformerRegistry.Register(new DateTimeComparisonExpressionTransformer());

            return transformerRegistry;
        }

        public static IQueryParser CreateQueryParser(IBucketContext bucketContext) =>
            new QueryParser(
                new ExpressionTreeParser(
                    _nodeTypeProvider,
                    new CompoundExpressionTreeProcessor(new IExpressionTreeProcessor[]
                    {
                        new PartialEvaluatingExpressionTreeProcessor(new NullEvaluatableExpressionFilter()),
                        SerializationExpressionTreeProcessor.FromBucketContext(bucketContext),
                        new TransformingExpressionTreeProcessor(_transformerRegistry)
                    })));
    }
}