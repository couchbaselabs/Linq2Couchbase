using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Operators;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Couchbase.Linq.Clauses
{
    /// <summary>
    /// Expression node for ExplainAsync.
    /// </summary>
    internal class ExplainAsyncExpressionNode : ResultOperatorExpressionNodeBase
    {
        /// <summary>
        /// Methods which are supported by this type of node.
        /// </summary>
        public static IEnumerable<MethodInfo> GetSupportedMethods() => new[]
        {
            QueryExtensionMethods.ExplainAsync
        };

        /// <summary>
        /// Creates a new ExplainAsyncExpressionNode.
        /// </summary>
        /// <param name="parseInfo">Method parse info.</param>
        public ExplainAsyncExpressionNode(MethodCallExpressionParseInfo parseInfo)
            : base(parseInfo, null, null)
        {
        }

        /// <inheritdoc />
        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            throw new NotSupportedException($"Resolve is not supported by {typeof(ExplainAsyncExpressionNode)}");
        }

        /// <inheritdoc />
        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext) =>
            new ExplainAsyncResultOperator();
    }
}
