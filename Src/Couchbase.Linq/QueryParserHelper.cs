using Couchbase.Linq.Clauses;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Operators;
using Couchbase.Linq.QueryGeneration.ExpressionTransformers;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

namespace Couchbase.Linq
{
    internal class QueryParserHelper
    {
        public static IQueryParser CreateQueryParser()
        {
            //Create Custom node registry
            var customNodeTypeRegistry = new MethodInfoBasedNodeTypeRegistry();

            //register the "Nest" clause type
            customNodeTypeRegistry.Register(NestExpressionNode.SupportedMethods,
                typeof(NestExpressionNode));

            //register the "Explain" expression node parser
            customNodeTypeRegistry.Register(ExplainExpressionNode.SupportedMethods,
                typeof(ExplainExpressionNode));

            //register the "UseKeys" expression node parser
            customNodeTypeRegistry.Register(UseKeysExpressionNode.SupportedMethods,
                typeof(UseKeysExpressionNode));

            //register the "ToQueryRequest" expression node parser
            customNodeTypeRegistry.Register(ToQueryRequestExpressionNode.SupportedMethods,
                typeof(ToQueryRequestExpressionNode));

            //register the "Set" and "Unset" expression node parser
            customNodeTypeRegistry.Register(UpdateExpressionNode.SupportedMethods,
                typeof(UpdateExpressionNode));

            //register the "Execute" expression node parser
            customNodeTypeRegistry.Register(ExecuteExpressionNode.SupportedMethods,
                typeof(ExecuteExpressionNode));

            //This creates all the default node types
            var nodeTypeProvider = ExpressionTreeParser.CreateDefaultNodeTypeProvider();

            //add custom node provider to the providers
            nodeTypeProvider.InnerProviders.Add(customNodeTypeRegistry);


            var transformerRegistry = ExpressionTransformerRegistry.CreateDefault();

            //Register transformer to handle enum == and != comparisons
            transformerRegistry.Register(new EnumComparisonExpressionTransformer());

            //Register transformer to handle string comparisons
            transformerRegistry.Register(new StringComparisonExpressionTransformer());

            //Register transformer to handle DateTime comparisons
            transformerRegistry.Register(new DateTimeComparisonExpressionTransformer());

            var processor = ExpressionTreeParser.CreateDefaultProcessor(transformerRegistry);
            var expressionTreeParser = new ExpressionTreeParser(nodeTypeProvider, processor);
            var queryParser = new QueryParser(expressionTreeParser);

            return queryParser;
        }
    }
}