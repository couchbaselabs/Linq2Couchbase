using Couchbase.Linq.Clauses;
using Couchbase.Linq.Extensions;
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
        private static readonly ExpressionTransformerRegistry _prePartialEvaluationTransformerRegistry;

        static QueryParserHelper()
        {
            _nodeTypeProvider = CreateDefaultNodeTypeProvider();
            _transformerRegistry = CreateDefaultTransformerRegistry();
            _prePartialEvaluationTransformerRegistry = CreatePrePartialEvaluationDefaultTransformerRegistry();
        }

        private static INodeTypeProvider CreateDefaultNodeTypeProvider()
        {
            //Create Custom node registry
            var nodeTypeRegistry = new MethodInfoBasedNodeTypeRegistry();

            //register the nodes for special Couchbase clauses
            nodeTypeRegistry.Register(NestExpressionNode.SupportedMethods, typeof(NestExpressionNode));
            nodeTypeRegistry.Register(ExplainExpressionNode.GetSupportedMethods(), typeof(ExplainExpressionNode));
            nodeTypeRegistry.Register(ExplainAsyncExpressionNode.GetSupportedMethods(), typeof(ExplainAsyncExpressionNode));
            nodeTypeRegistry.Register(UseKeysExpressionNode.SupportedMethods, typeof(UseKeysExpressionNode));
            nodeTypeRegistry.Register(UseIndexExpressionNode.SupportedMethods, typeof(UseIndexExpressionNode));
            nodeTypeRegistry.Register(UseHashExpressionNode.SupportedMethods, typeof(UseHashExpressionNode));
            nodeTypeRegistry.Register(ScanConsistencyExpressionNode.GetSupportedMethods(), typeof(ScanConsistencyExpressionNode));
            nodeTypeRegistry.Register(ConsistentWithExpressionNode.GetSupportedMethods(), typeof(ConsistentWithExpressionNode));

            //register the various asynchronous expression nodes
            nodeTypeRegistry.Register(FirstAsyncExpressionNode.GetSupportedMethods(), typeof(FirstAsyncExpressionNode));
            nodeTypeRegistry.Register(SingleAsyncExpressionNode.GetSupportedMethods(), typeof(SingleAsyncExpressionNode));
            nodeTypeRegistry.Register(AnyAsyncExpressionNode.GetSupportedMethods(), typeof(AnyAsyncExpressionNode));
            nodeTypeRegistry.Register(AllAsyncExpressionNode.GetSupportedMethods(), typeof(AllAsyncExpressionNode));
            nodeTypeRegistry.Register(CountAsyncExpressionNode.GetSupportedMethods(), typeof(CountAsyncExpressionNode));
            nodeTypeRegistry.Register(LongCountAsyncExpressionNode.GetSupportedMethods(), typeof(LongCountAsyncExpressionNode));
            nodeTypeRegistry.Register(SumAsyncExpressionNode.GetSupportedMethods(), typeof(SumAsyncExpressionNode));
            nodeTypeRegistry.Register(AverageAsyncExpressionNode.GetSupportedMethods(), typeof(AverageAsyncExpressionNode));
            nodeTypeRegistry.Register(MinAsyncExpressionNode.GetSupportedMethods(), typeof(MinAsyncExpressionNode));
            nodeTypeRegistry.Register(MaxAsyncExpressionNode.GetSupportedMethods(), typeof(MaxAsyncExpressionNode));

            //This creates all the default node types
            var nodeTypeProvider = ExpressionTreeParser.CreateDefaultNodeTypeProvider();

            //add custom node provider to the providers
            nodeTypeProvider.InnerProviders.Add(nodeTypeRegistry);

            return nodeTypeProvider;
        }

        private static ExpressionTransformerRegistry CreateDefaultTransformerRegistry()
        {
            var transformerRegistry = ExpressionTransformerRegistry.CreateDefault();

            //Register transformer to handle string comparisons
            transformerRegistry.Register(new StringComparisonExpressionTransformer());

            //Register transformer to handle DateTime comparisons
            transformerRegistry.Register(new DateTimeComparisonExpressionTransformer());
            transformerRegistry.Register(new DateTimeSortExpressionTransformer());

            return transformerRegistry;
        }

        private static ExpressionTransformerRegistry CreatePrePartialEvaluationDefaultTransformerRegistry()
        {
            var transformerRegistry = ExpressionTransformerRegistry.CreateDefault();

            //Register transformer to handle enum == and != comparisons
            transformerRegistry.Register(new EnumComparisonExpressionTransformer());

            return transformerRegistry;
        }

        public static IQueryParser CreateQueryParser(ICluster cluster) =>
            new QueryParser(
                new ExpressionTreeParser(
                    _nodeTypeProvider,
                    new CompoundExpressionTreeProcessor(new IExpressionTreeProcessor[]
                    {
                        new TransformingExpressionTreeProcessor(_prePartialEvaluationTransformerRegistry),
                        SerializationExpressionTreeProcessor.FromCluster(cluster),
                        new PartialEvaluatingExpressionTreeProcessor(new ExcludeSerializationConversionEvaluatableExpressionFilter()),
                        new TransformingExpressionTreeProcessor(_transformerRegistry)
                    })));
    }
}
