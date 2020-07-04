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
    /// Expression node for MaxAsync.
    /// </summary>
    internal class MaxAsyncExpressionNode : ResultOperatorExpressionNodeBase
    {
        /// <summary>
        /// Methods which are supported by this type of node.
        /// </summary>
        public static IEnumerable<MethodInfo> GetSupportedMethods() => new[]
        {
            QueryExtensionMethods.MaxAsyncNoSelector,
            QueryExtensionMethods.MaxAsyncWithSelector
        };

        /// <summary>
        /// Creates a new MaxAsyncExpressionNode.
        /// </summary>
        /// <param name="parseInfo">Method parse info.</param>
        /// <param name="optionalSelector">Optional selector for value.</param>
        public MaxAsyncExpressionNode(MethodCallExpressionParseInfo parseInfo, LambdaExpression optionalSelector)
            : base(parseInfo, null, optionalSelector)
        {
        }

        /// <inheritdoc />
        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            throw new NotSupportedException($"Resolve is not supported by {typeof(MaxAsyncExpressionNode)}");
        }

        /// <inheritdoc />
        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext) =>
            new MaxAsyncResultOperator();
    }
}
