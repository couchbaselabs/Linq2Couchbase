using Couchbase.Linq.Clauses;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Operators;
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

            //This creates all the default node types
            var nodeTypeProvider = ExpressionTreeParser.CreateDefaultNodeTypeProvider();

            //add custom node provider to the providers
            nodeTypeProvider.InnerProviders.Add(customNodeTypeRegistry);


            var transformerRegistry = ExpressionTransformerRegistry.CreateDefault();
            var processor = ExpressionTreeParser.CreateDefaultProcessor(transformerRegistry);
            var expressionTreeParser = new ExpressionTreeParser(nodeTypeProvider, processor);
            var queryParser = new QueryParser(expressionTreeParser);

            return queryParser;
        }
    }
}