using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.Extensions;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

namespace Couchbase.Linq
{
    public class QueryParserHelper
    {
        public static IQueryParser CreateQueryParser()
        {
            var customNodeTypeRegistry = new MethodInfoBasedNodeTypeRegistry();

            customNodeTypeRegistry.Register(WhereMissingExpressionNode.SupportedMethods, typeof(WhereMissingExpressionNode));

            var nodeTypeProvider = ExpressionTreeParser.CreateDefaultNodeTypeProvider();

            nodeTypeProvider.InnerProviders.Add(customNodeTypeRegistry);

            var transformerRegistry = ExpressionTransformerRegistry.CreateDefault();

            var processor = ExpressionTreeParser.CreateDefaultProcessor(transformerRegistry);

            var expressionTreeParser = new ExpressionTreeParser(nodeTypeProvider, processor);

            var queryParser = new QueryParser(expressionTreeParser);

            return queryParser;
        }
    }
}
