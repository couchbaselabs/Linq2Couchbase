using System.Linq.Expressions;
using Couchbase.Linq.Utils;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;
using Remotion.Linq.Parsing.Structure;

namespace Couchbase.Linq.QueryGeneration
{
    internal sealed class EnhancedPartialEvaluatingExpressionTreeProcessor : IExpressionTreeProcessor
    {
        public IEvaluatableExpressionFilter Filter { get; }

        public EnhancedPartialEvaluatingExpressionTreeProcessor(IEvaluatableExpressionFilter filter)
        {
            ThrowHelpers.ThrowIfNull(filter);

            Filter = filter;
        }

        public Expression? Process(Expression expressionTree)
        {
            ThrowHelpers.ThrowIfNull(expressionTree);

            return EnhancedPartialEvaluatingExpressionVisitor.EvaluateIndependentSubtrees(expressionTree, Filter);
        }
    }
}
